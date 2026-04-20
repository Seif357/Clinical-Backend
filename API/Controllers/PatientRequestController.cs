using System.Security.Claims;
using Application.Dto.Communication;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientRequestController(IPatientRequestService patientRequestService) : ControllerBase
{
    // GET /api/PatientRequest — summarized list
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientRequestService.GetAllSummaryAsync(id);
    }

    // GET /api/PatientRequest/{id} — full detail + embedded DoctorResponses
    [HttpGet("{requestId:int}")]
    public async Task<IActionResult> GetById(int requestId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientRequestService.GetByIdAsync(id, requestId);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePatientRequestDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientRequestService.CreateAsync(id, dto);
    }

    [HttpPut("{requestId:int}")]
    public async Task<IActionResult> Update(int requestId, [FromForm] UpdatePatientRequestDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientRequestService.UpdateAsync(id, requestId, dto);
    }

    [HttpDelete("{requestId:int}")]
    public async Task<IActionResult> Delete(int requestId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientRequestService.DeleteAsync(id, requestId);
    }
}