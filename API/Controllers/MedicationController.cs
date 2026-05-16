using System.Security.Claims;
using Application.Dto.MedicalRecord;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/medications")]
[Authorize]
public class MedicationController(IMedicationService medicationService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await medicationService.GetPatientMedicationsAsync(userId.Value);
    }
    
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await medicationService.GetByIdAsync(userId.Value, id);
    }
    
    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Add([FromBody] AddMedicationDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await medicationService.AddSelfAsync(userId.Value, dto);
    }
    
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMedicationDto dto)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await medicationService.UpdateAsync(userId.Value, id, dto);
    }
    
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();
        return await medicationService.DeleteAsync(userId.Value, id);
    }
    
    [HttpGet("patient/{patientUserId:int}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetForPatient(int patientUserId)
    {
        var doctorId = GetUserId();
        if (doctorId is null) return Unauthorized();
        return await medicationService.GetForPatientAsync(doctorId.Value, patientUserId);
    }

    private int? GetUserId()
    {
        var val = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(val, out var id) ? id : null;
    }
}