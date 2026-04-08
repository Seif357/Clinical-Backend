using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService,
    IJwtService jwtService,
    IWebHostEnvironment environment) : ControllerBase
{
    [HttpPost("register")]
    [Produces(typeof(Result))]
    [AllowAnonymous]
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
            SetRefreshTokenCookie(result.RefreshToken);

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
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthResult), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await authService.LoginServiceAsync(dto);
        if (!result.Success)
        {
            return result.Message==AuthConstants.Messages.InvalidCredentials
                ? Unauthorized(result) 
                : BadRequest(result);
        }
        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            SetRefreshTokenCookie(result.RefreshToken);
        }
        result.RefreshToken = HttpContext.Request.Headers["X-Client-Type"].ToString().Equals("server", StringComparison.OrdinalIgnoreCase) ? result.RefreshToken : null;
        return Ok(result);
    }

    [HttpPut("updateMail")]
    [Produces(typeof(IActionResult))]
    [Authorize]
    public async Task<IActionResult> UpdateEmail(UpdateEmailDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await authService.UpdateEmailServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPut("updateUsername")]
    [Produces(typeof(IActionResult))]
    [Authorize]
    public async Task<IActionResult> UpdateUsername(UpdateUsernameDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        //var refreshToken = Request.Cookies["refreshToken"];
        //    var refreshResult = await _auth.RefreshTokenAsync(refreshToken);
        //    if (!refreshResult.Success)
        //    {
        //    }
        //userId = refreshResult.AccessToken.     
        var result = await authService.UpdateUsernameServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("refresh-token")]
    [Produces(typeof(IActionResult))]
    public async Task<IActionResult> RefreshToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result = await authService.RefreshTokenAsync(refreshToken);
            if (!result.Success) return BadRequest(result);
            Response.Cookies
                .Append("refreshToken", result.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = result.RefreshTokenExpiration,
                    IsEssential = true
                });
            result.RefreshToken = null;
            return Ok(result);
        }
        catch (SecurityTokenException e)
        {
            return Unauthorized(new { e.Message });
        }
        //TODO: Handle other exceptions that are already handled in the service layer
        //    catch (Exception e)
        //    {
        //        return StatusCode(500, new { Message = $"error occured during Refresh token" });
        //    }
    }

    [HttpPost("logout")]
    [Produces(typeof(IActionResult))]
    [Authorize]
    //todo: add logging out from one device only
    public async Task<IActionResult> RevokeToken()
    {
        try
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var success = await authService.RevokeTokenServiceAsync(refreshToken, "Logged out");
            if (success)
            {
                DeleteRefreshTokenCookie();
                return Ok();
            }

            return BadRequest();
        }
        catch (SecurityTokenException e)
        {
            return Unauthorized(new { e.Message });
        }
        //TODO: Handle other exceptions that are already handled in the service layer
        //catch (Exception e)
        //{
        //    return StatusCode(500, new { Message = "error occured during Revoke token" });
        //}
    }

    [HttpPut("updatePassword")]
    [Produces(typeof(IActionResult))]
    public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await authService.UpdatePasswordServiceAsync(userId, dto);
        if (!result.Success) return BadRequest(result);
        return Ok(result);
    }

    private void SetRefreshTokenCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(), 
            SameSite = SameSiteMode.Lax, // Lax allows the cookie to be sent on navigation
            Expires = DateTime.UtcNow.AddDays(jwtService.GetRefreshTokenExpirationDays()),
            Path = "/", 
            IsEssential = true
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
    private void DeleteRefreshTokenCookie()
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Lax,
            Expires = DateTime.UtcNow.AddDays(-1),
            Path = "/" 
        };

        Response.Cookies.Delete("refreshToken", cookieOptions);
    }
    [HttpDelete("delete")]
    [Produces(typeof(Result))]
    [Authorize]
    public async Task<IActionResult> DeleteAccount()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var delResult = await authService.DeleteAccountService(userId);
        if (!delResult.Success) return BadRequest(delResult);

        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await authService.RevokeTokenServiceAsync(refreshToken, "Account deleted");
                DeleteRefreshTokenCookie();
            }
            catch (SecurityTokenException)
            {
                // Token already revoked or invalid — account is still deleted
            }
        }

        return Ok(delResult);
    }
}