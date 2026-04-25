using System.Security.Claims;
using Application.Dto;
using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DoctorController(IDoctorService doctorService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetDoctorData()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await doctorService.GetDoctorDataServiceAsync(id);
        return result;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdateDoctorData(UpdateDoctorDto updateDoctorDto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await doctorService.UpdateDoctorDataServiceAsync(id, updateDoctorDto);
        return result;
    }
    /// <summary>
    /// Search all approved doctors.
    /// Available to: Patients, Doctors, Admins (and anonymous if you want — remove [Authorize]).
    /// </summary>
    [HttpGet("DoctorSearch")]
    [Authorize]
    [ProducesResponseType(typeof(Result), 200)]
    public async Task<IActionResult> GetDoctors([FromQuery] DoctorSearchQuery query)
        => Ok(await doctorService.SearchDoctorsAsync(query));
}