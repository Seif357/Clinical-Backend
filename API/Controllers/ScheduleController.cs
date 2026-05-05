using System.Security.Claims;
using Application.Dto.Schedule;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/schedule")]
[Authorize]
public class ScheduleController(IScheduleService scheduleService) : ControllerBase
{
    [HttpPost("slots/generate")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> GenerateSlots([FromBody] GenerateSlotsDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.GenerateSlotsAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPost("slots")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> CreateSlot([FromBody] CreateSlotDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.CreateSlotAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpDelete("slots/{slotId:int}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> DeleteSlot(int slotId)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.DeleteSlotAsync(userId.Value, slotId);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpPatch("slots/complete")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> CompleteSlot([FromBody] CompleteSlotDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.CompleteSlotAsync(userId.Value, dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("my")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetMySchedule()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.GetMyScheduleAsync(userId.Value);
        return Ok(result);
    }
    
    [HttpGet("my/daily/{date}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetDailySchedule(DateOnly date)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.GetDailyScheduleAsync(userId.Value, date);
        return Ok(result);
    }

    [HttpGet("my/weekly/{weekStart}")]
    [Authorize(Roles = "Doctor")]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetWeeklySchedule(DateOnly weekStart)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        var result = await scheduleService.GetWeeklyScheduleAsync(userId.Value, weekStart);
        return Ok(result);
    }

    private int? GetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}