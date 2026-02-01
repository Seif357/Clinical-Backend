using Application.Common.Interfaces;
using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Mapper;
using Application.Services;
using Domain.Models;
using Infrastructure.DataAccess;
using Infrastructure.DataAccess.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController: ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _auth;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _logger = logger;
            _auth = authService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await _auth.RegisterServiceAsync(registerDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("registeration-login")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterationLogin([FromBody] RegisterDto registerDto)
        {
            var result = await _auth.RegisterServiceAsync(registerDto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            var dto = new LoginDto(
                registerDto.Email,
                registerDto.Password
            );

            var authResult = await _auth.LoginServiceAsync(dto);
            if (!authResult.Success)
            {
                return BadRequest(result);
            }
            Response.Cookies
    .Append("refreshToken", authResult.RefreshToken, new CookieOptions
    {
        HttpOnly = true,
        Secure = true,
        SameSite = SameSiteMode.None,
        Expires = authResult.RefreshTokenExpiration,
        IsEssential = true
    });
            authResult.RefreshToken = null;
            return Ok(result);
        }
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _auth.LoginServiceAsync(dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
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
        [HttpPut("updateMail")]
        [Authorize]
        public async Task<IActionResult> UpdateEmail(UpdateEmailDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _auth.UpdateEmailServiceAsync(userId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPut("updateUsername")]
        [Authorize]
        public async Task<IActionResult> UpdateUsername(UpdateUsernameDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            //var refreshToken = Request.Cookies["refreshToken"];
            //    var refreshResult = await _auth.RefreshTokenAsync(refreshToken);
            //    if (!refreshResult.Success)
            //    {
            //    }
                //userId = refreshResult.AccessToken.     
            var result = await _auth.UpdateUsernameServiceAsync(userId,dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var result = await _auth.RefreshTokenAsync(refreshToken);
                if (!result.Success)
                {
                    return BadRequest(result);
                }
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
        [Authorize]
        //todo: add logging out from one device only
        public async Task<IActionResult> RevokeToken()
        {
            try
            {
                var refreshToken = Request.Cookies["refreshToken"];
                var success = await _auth.RevokeTokenServiceAsync(refreshToken, "Logged out");
                if (success)
                {
                    Response.Cookies.Delete("refreshToken");
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
        public async Task<IActionResult> UpdatePassword(int id, UpdatePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            var result = await _auth.UpdatePasswordServiceAsync(userId, dto);
            if (!result.Success)
            {
                return BadRequest(result);
            }
            return Ok(result);
        }

    }
}