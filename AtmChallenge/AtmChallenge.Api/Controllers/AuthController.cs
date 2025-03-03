using AtmChallenge.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using AtmChallenge.Application.Interfaces;

using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AtmChallenge.API.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IConfiguration _configuration;
    private readonly ICryptoService _cryptoService;

    public AuthController(IUserService userService, ICryptoService cryptoService, IConfiguration configuration)
    {
        _userService = userService;
        _cryptoService = cryptoService;
        _configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {   
        var encryptedCardNumber = _cryptoService.EncryptData(login.CardNumber);
        var encryptedPin = _cryptoService.EncryptData(login.Pin);
        
        if (await _userService.IsCardNumberLockedOutAsync(encryptedCardNumber) == null)
        {
            return Unauthorized(new { message = "The Card is locked out." });
        }

        var user = await _userService.AuthenticateUserAsync(encryptedCardNumber, encryptedPin);
        if (user == null)
        {
            await _userService.RecordFailedLoginAsync(encryptedCardNumber);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        await _userService.ResetFailedAttemptsAsync(encryptedCardNumber);
        return Ok(new { message = "Login successful!" });
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