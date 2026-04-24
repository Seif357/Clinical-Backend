using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.Dto.Email_management;
using Application.Dto.Phone_management;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Manages multiple emails and phone numbers per user.
/// Both patients (mobile) and doctors (web) share these endpoints.
/// </summary>
[ApiController]
[Route("api/auth/contacts")]
[Authorize]
public class ContactController(IContactService contactService) : ControllerBase
{
    // ── Email ─────────────────────────────────────────────────────────────────

    [HttpPost("email")]
    public async Task<IActionResult> AddEmail([FromBody] AddEmailDto dto)
        => ToResponse(await contactService.AddEmailAsync(GetUserId(), GetDisplayName(), dto));

    [HttpPost("email/verify")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailDto dto)
        => ToResponse(await contactService.VerifyEmailAsync(GetUserId(), dto));

    [HttpPost("email/resend")]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationDto dto)
        => ToResponse(await contactService.ResendEmailVerificationAsync(GetUserId(), dto));

    [HttpPut("email/primary")]
    public async Task<IActionResult> SetPrimaryEmail([FromBody] SetPrimaryEmailDto dto)
        => ToResponse(await contactService.SetPrimaryEmailAsync(GetUserId(), dto));

    [HttpDelete("email")]
    public async Task<IActionResult> RemoveEmail([FromBody] RemoveEmailDto dto)
        => ToResponse(await contactService.RemoveEmailAsync(GetUserId(), dto));

    // ── Phone ─────────────────────────────────────────────────────────────────

    [HttpPost("phone")]
    public async Task<IActionResult> AddPhone([FromBody] AddPhoneDto dto)
        => ToResponse(await contactService.AddPhoneAsync(GetUserId(), GetDisplayName(), dto));

    [HttpPost("phone/verify")]
    public async Task<IActionResult> VerifyPhone([FromBody] VerifyPhoneDto dto)
        => ToResponse(await contactService.VerifyPhoneAsync(GetUserId(), dto));

    [HttpPost("phone/resend")]
    public async Task<IActionResult> ResendPhoneVerification([FromBody] ResendPhoneVerificationDto dto)
        => ToResponse(await contactService.ResendPhoneVerificationAsync(GetUserId(), dto));

    [HttpPut("phone/primary")]
    public async Task<IActionResult> SetPrimaryPhone([FromBody] SetPrimaryPhoneDto dto)
        => ToResponse(await contactService.SetPrimaryPhoneAsync(GetUserId(), dto));

    [HttpDelete("phone")]
    public async Task<IActionResult> RemovePhone([FromBody] RemovePhoneDto dto)
        => ToResponse(await contactService.RemovePhoneAsync(GetUserId(), dto));

    // ── Helpers ───────────────────────────────────────────────────────────────
    private IActionResult ToResponse(Result r) => r.Success ? Ok(r) : BadRequest(r);
    private int  GetUserId()      => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private string GetDisplayName() => User.FindFirstValue(ClaimTypes.Name) ?? "User";
}