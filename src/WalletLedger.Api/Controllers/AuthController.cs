using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WalletLedger.Api.Contracts.Auth;
using WalletLedger.Application.Auth;

namespace WalletLedger.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        await authService.RegisterAsync(request.Email, request.Password, cancellationToken);
        return StatusCode(StatusCodes.Status201Created);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var tokens = await authService.LoginAsync(request.Email, request.Password, ClientIp(), cancellationToken);
        return Ok(TokenResponse.From(tokens));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<TokenResponse>> Refresh(
        RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var tokens = await authService.RefreshAsync(request.RefreshToken, ClientIp(), cancellationToken);
        return Ok(TokenResponse.From(tokens));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<MeResponse> Me() =>
        Ok(new MeResponse(
            User.FindFirst(WalletClaims.Subject)?.Value,
            User.FindAll(WalletClaims.Role).Select(claim => claim.Value).ToList()));

    private string? ClientIp() => HttpContext.Connection.RemoteIpAddress?.ToString();
}