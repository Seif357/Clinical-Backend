using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.Dto.Doctor_approval;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/admin/doctors")]
[Authorize(Roles = "Admin")]
public class AdminDoctorController(IDoctorAdminService doctorAdminService) : ControllerBase
{
    /// <summary>
    /// Get all pending (unreviewed) doctor registrations.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetPending([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await doctorAdminService.GetPendingDoctorsAsync(page, pageSize));

    /// <summary>
    /// Get all approved doctors (full list, same as what patients see).
    /// </summary>
    [HttpGet("approved")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetApproved([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => Ok(await doctorAdminService.GetApprovedDoctorsAsync(page, pageSize));

    /// <summary>
    /// Get a specific doctor registration with all details.
    /// </summary>
    [HttpGet("{doctorUserId:int}")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 404)]
    public async Task<IActionResult> GetRegistration([FromRoute] int doctorUserId)
    {
        var result = await doctorAdminService.GetDoctorRegistrationAsync(doctorUserId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Approve a doctor. They become visible to patients immediately.
    /// </summary>
    [HttpPost("{doctorUserId:int}/approve")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Approve([FromRoute] int doctorUserId)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result  = await doctorAdminService.ApproveDoctorAsync(adminId, new ApproveDoctorDto(doctorUserId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Reject a doctor registration with a reason.
    /// </summary>
    [HttpPost("{doctorUserId:int}/reject")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Reject([FromRoute] int doctorUserId, [FromBody] RejectReasonDto dto)
    {
        var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result  = await doctorAdminService.RejectDoctorAsync(adminId, new RejectDoctorDto(doctorUserId, dto.Reason));
        return result.Success ? Ok(result) : BadRequest(result);
    }
}

public record RejectReasonDto(string Reason);