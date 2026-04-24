using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth/password")]
public class PasswordController(IPasswordService passwordService) : ControllerBase
{
    /// <summary>
    /// Step 1 (authenticated): verify current password and send OTP to primary email.
    /// </summary>
    [HttpPost("change/request")]
    [Authorize]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> RequestChange([FromBody] RequestPasswordChangeOtpDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await passwordService.RequestPasswordChangeOtpAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Step 2 (authenticated): submit OTP + new password. Revokes all other sessions.
    /// </summary>
    [HttpPost("change/confirm")]
    [Authorize]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> ConfirmChange([FromBody] ConfirmPasswordChangeDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await passwordService.ConfirmPasswordChangeAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Step 1 (unauthenticated): request a forgot-password OTP.
    /// Always returns 200 to avoid leaking account existence.
    /// </summary>
    [HttpPost("forgot")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> ForgotRequest([FromBody] ForgotPasswordRequestDto dto)
    {
        var result = await passwordService.ForgotPasswordRequestAsync(dto);
        return Ok(result); // always 200
    }

    /// <summary>
    /// Step 2 (unauthenticated): verify OTP and set the new password.
    /// </summary>
    [HttpPost("reset")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Reset([FromBody] ForgotPasswordResetDto dto)
    {
        var result = await passwordService.ForgotPasswordResetAsync(dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private int? GetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}