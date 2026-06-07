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

    /// <summary>
    /// Mark a doctor request as Completed.
    /// Only the doctor who owns the request can complete it.
    /// </summary>
    [HttpPatch("{requestId:int}/complete")]
    public async Task<IActionResult> MarkComplete(int requestId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.MarkCompleteAsync(id, requestId);
    }
    /// <summary>
    /// (Patient) Get all doctor requests directed to the authenticated patient.
    /// </summary>
    [HttpGet("incoming")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetIncoming()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorRequestService.GetIncomingForPatientAsync(id);
    }
}