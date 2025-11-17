using Microsoft.AspNetCore.Mvc;
using LunchSystem.DTOs;
using LunchSystem.Services;

namespace LunchSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
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
            return Unauthorized(new { message = "Email ou senha inválidos" });

        return Ok(response);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);
        if (!result)
            return NotFound(new { message = "Email não encontrado" });

        return Ok(new { message = "Token de recuperação enviado por email" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        if (!result)
            return BadRequest(new { message = "Token inválido ou expirado" });

        return Ok(new { message = "Senha alterada com sucesso" });
    }
}
