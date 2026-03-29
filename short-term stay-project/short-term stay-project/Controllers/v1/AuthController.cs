using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using short_term_stay_project.DTOs;
using short_term_stay_project.Services;

namespace short_term_stay_project.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        if (response == null)
            return Unauthorized("Invalid credentials.");

        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var success = await _authService.RegisterAsync(request);
        if (!success)
            return BadRequest("Registration failed (user might exist).");

        return Ok("User registered successfully.");
    }
}
