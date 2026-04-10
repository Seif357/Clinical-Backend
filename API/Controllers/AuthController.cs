using System.Security.Claims;
using API.Helpers;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService,
    IJwtService jwtService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
    {
        var result = await authService.RegisterServiceAsync(registerDto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Sign in (or sign up) using a Google id_token obtained by the client via Google Sign-In.
    /// On success the refresh token is set as an HttpOnly cookie, just like the standard login.
    /// Doctors must also supply ProfessionalPracticeLicense and IssuingAuthority on first sign-up.
    /// </summary>
    [HttpPost("google-login")]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
    {
        var result = await authService.GoogleLoginServiceAsync(dto);
        if (!result.Success) return BadRequest(result);

        if (!string.IsNullOrEmpty(result.RefreshToken))
            AuthHelper.SetRefreshTokenCookie(Response, result.RefreshToken, result.RefreshTokenExpiration, environment, jwtService.GetRefreshTokenExpirationDays());
        result.RefreshToken = HttpContext.Request.Headers["X-Client-Type"]
            .ToString().Equals("server", StringComparison.OrdinalIgnoreCase)
                ? result.RefreshToken
                : null;

        return Ok(result);
    }

    /// <summary>
    /// Login with UserName or Email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody]LoginDto dto)
    {
        var result = await authService.LoginServiceAsync(dto);
        if (!result.Success)
        {
            return result.Message==AuthConstants.Messages.InvalidCredentials
                ? Unauthorized(result) 
                : BadRequest(result);
        }
        AuthHelper.SetRefreshTokenCookie(Response, result.RefreshToken, result.RefreshTokenExpiration, environment, jwtService.GetRefreshTokenExpirationDays());
        result.RefreshToken = HttpContext.Request.Headers["X-Client-Type"].ToString()
            .Equals("server", StringComparison.OrdinalIgnoreCase)
            ? result.RefreshToken
            : null;
        return Ok(result);
    }

    [HttpPut("updateMail")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await authService.UpdateEmailServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("updateUsername")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateUsername([FromBody] UpdateUsernameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await authService.UpdateUsernameServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken()
    {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await authService.RefreshTokenAsync(refreshToken);
            if (!result.Success) return BadRequest(result);
            AuthHelper.SetRefreshTokenCookie(Response, result.RefreshToken, result.RefreshTokenExpiration, environment, jwtService.GetRefreshTokenExpirationDays());
            result.RefreshToken = null;
            return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    //todo: add logging out from one device only
    public async Task<IActionResult> RevokeToken()
    {
            var refreshToken = Request.Cookies["refreshToken"];
            var success = await authService.RevokeTokenServiceAsync(refreshToken, "Logged out");
            if (success)
            {
                AuthHelper.DeleteRefreshTokenCookie(Response, environment);
                return Ok();
            }
            else
            {
                return BadRequest();
            }
    }

    [HttpPut("updatePassword")]
    [Authorize]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdatePassword([FromBody]UpdatePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await authService.UpdatePasswordServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }
    
    [HttpDelete("delete")]
    [Authorize]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteAccount([FromBody]DeleteAccountDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var result = await authService.DeleteAccountService(userId, dto.Password);
        if (!result.Success) return BadRequest(result);

        AuthHelper.DeleteRefreshTokenCookie(Response, environment);
        return Ok(result);
    }
}