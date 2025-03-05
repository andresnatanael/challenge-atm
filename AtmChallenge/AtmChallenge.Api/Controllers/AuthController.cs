using AtmChallenge.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using AtmChallenge.Application.Interfaces;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using AtmChallenge.Application.Exceptions;

namespace AtmChallenge.API.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ICryptoService _cryptoService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ICryptoService cryptoService, IConfiguration configuration, ILogger<AuthController> logger)
    {
        _userService = userService;
        _cryptoService = cryptoService;
        _configuration = configuration;
        _logger = logger;
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        _logger.LogInformation("Login request received.");
        var encryptedCardNumber = _cryptoService.EncryptData(login.CardNumber);
        var encryptedPin = _cryptoService.EncryptData(login.Pin);
        try
        {
            var cardLocked = await _userService.IsCardNumberLockedOutAsync(encryptedCardNumber);
            if (cardLocked == true)
            {
                return Unauthorized(new { message = "Card locked out." });
            }

            var user = await _userService.AuthenticateUserAsync(encryptedCardNumber, encryptedPin);
            await _userService.ResetFailedAttemptsAsync(encryptedCardNumber);

            string jwtToken = GenerateJwtToken(user.Username, user.Id, encryptedCardNumber);

            return Ok(new
            {
                access_token = jwtToken,
                type = "Bearer",
                expires_in = 600,
            });
        }
        catch (CardNotFoundException ex)
        {
            _logger.LogError(ex, "Card not found.");
            return Unauthorized(new { message = "Card not found." });
        }
        catch (InvalidCredentialException ex)
        {
            await _userService.RecordFailedLoginAsync(encryptedCardNumber);
            _logger.LogError(ex, "Invalid credentials provided.");
            return Unauthorized(new { message = "Invalid credentials provided." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
    
    private string GenerateJwtToken(string userName, int userId, string encryptedCard)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("UserId", userId.ToString()),
            new Claim("EncryptedCard", encryptedCard)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(10),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}