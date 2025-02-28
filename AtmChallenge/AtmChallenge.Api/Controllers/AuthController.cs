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
        if (await _userService.IsUserLockedOutAsync(login.Username))
        {
            return Unauthorized(new { message = "User is locked out." });
        }

        var user = await _userService.AuthenticateUserAsync(login.Username, login.Password);
        if (user == null)
        {
            await _userService.RecordFailedLoginAsync(login.Username);
            return Unauthorized(new { message = "Invalid credentials." });
        }

        await _userService.ResetFailedAttemptsAsync(login.Username);
        return Ok(new { message = "Login successful!" });
    }
}