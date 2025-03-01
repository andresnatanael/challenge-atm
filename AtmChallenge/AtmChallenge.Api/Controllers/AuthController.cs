using AtmChallenge.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using AtmChallenge.Application.Interfaces;


[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest login)
    {
        if (await _userService.IsCardNumberLockedOutAsync(login.CardNumber))
        {
            return Unauthorized(new { message = "The Card is locked out." });
        }

        var user = await _userService.AuthenticateUserAsync(login.CardNumber, login.Pin);
        if (user == null)
        {
            await _userService.RecordFailedLoginAsync(login.CardNumber);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        await _userService.ResetFailedAttemptsAsync(login.CardNumber);
        return Ok(new { message = "Login successful!" });
    }
}