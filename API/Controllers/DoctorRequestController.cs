using System.Security.Claims;
using Application.Dto.Communication;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorRequestController(IDoctorRequestService doctorRequestService) : ControllerBase
{
    // GET /api/DoctorRequest — summarized list
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.GetAllSummaryAsync(id);
    }

    // GET /api/DoctorRequest/{id} — full detail + embedded PatientResponses
    [HttpGet("{requestId:int}")]
    public async Task<IActionResult> GetById(int requestId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.GetByIdAsync(id, requestId);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreateDoctorRequestDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.CreateAsync(id, dto);
    }

    [HttpPut("{requestId:int}")]
    public async Task<IActionResult> Update(int requestId, [FromForm] UpdateDoctorRequestDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.UpdateAsync(id, requestId, dto);
    }

    [HttpDelete("{requestId:int}")]
    public async Task<IActionResult> Delete(int requestId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.DeleteAsync(id, requestId);
    }
}
