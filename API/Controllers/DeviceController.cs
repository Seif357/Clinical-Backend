using System.Security.Claims;
using Application.Dto.AuthDto;
using Application.Dto.Device_management;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/auth/devices")]
[Authorize]
public class DeviceController(IDeviceService deviceService) : ControllerBase
{
    /// <summary>List all active devices for the current user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetDevices()
    {
        // The current device id is stored in the JWT as a claim (see login flow below).
        var userId        = GetUserId();
        var currentDevice = GetCurrentDeviceId();
        var result = await deviceService.GetUserDevicesAsync(userId, currentDevice);
        return Ok(result);
    }

    /// <summary>Give a device a friendly name ("My iPhone", "Work laptop").</summary>
    [HttpPut("rename")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> RenameDevice([FromBody] RenameDeviceDto dto)
    {
        var result = await deviceService.RenameDeviceAsync(GetUserId(), dto);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Log out from a specific device. Revokes its refresh tokens;
    /// the next request from that device will force a re-login.
    /// </summary>
    [HttpDelete("{deviceId:int}")]
    [ProducesResponseType(typeof(Result), 200)]
    [ProducesResponseType(typeof(Result), 400)]
    public async Task<IActionResult> LogoutDevice([FromRoute] int deviceId)
    {
        var result = await deviceService.LogoutDeviceAsync(GetUserId(), new LogoutDeviceDto(deviceId));
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private int GetCurrentDeviceId()
    {
        var val = User.FindFirstValue("device_id");
        return int.TryParse(val, out var id) ? id : 0;
    }
}