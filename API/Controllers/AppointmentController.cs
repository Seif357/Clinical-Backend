using System.Security.Claims;
using Application.Dto.Schedule;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize]
public class AppointmentController(IScheduleService scheduleService) : ControllerBase
{
    /// <summary>
    /// (Patient) Browse all available slots for a specific doctor.
    /// </summary>
    [HttpGet("available/{doctorUserId:int}")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetAvailableSlots(int doctorUserId)
    {
        var result = await scheduleService.GetAvailableSlotsAsync(doctorUserId);
        return Ok(result);
    }

    /// <summary>
    /// (Patient) Book an available slot.
    /// </summary>
    [HttpPost("book")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Book([FromBody] BookSlotDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.BookSlotAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// (Patient) Reschedule an existing appointment to another available slot (same doctor).
    /// </summary>
    [HttpPatch("reschedule")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Reschedule([FromBody] RescheduleDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.RescheduleAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// (Patient or Doctor) Cancel a booked appointment.
    /// </summary>
    [HttpPatch("cancel")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> Cancel([FromBody] CancelSlotDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.CancelSlotAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// (Patient) Get all upcoming booked appointments for the logged-in patient.
    /// </summary>
    [HttpGet("my")]
    [Authorize(Roles = "Patient")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.GetMyAppointmentsAsync(userId.Value);
        return Ok(result);
    }

    private int? GetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}