using System.Security.Claims;
using Application.Dto.Communication;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PatientResponseController(IPatientResponseService patientResponseService) : ControllerBase
{
    // No GET — responses are fetched via GET /api/DoctorRequest/{id}

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePatientResponseDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientResponseService.CreateAsync(id, dto);
    }

    [HttpPut("{responseId:int}")]
    public async Task<IActionResult> Update(int responseId, [FromForm] UpdatePatientResponseDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientResponseService.UpdateAsync(id, responseId, dto);
    }

    [HttpDelete("{responseId:int}")]
    public async Task<IActionResult> Delete(int responseId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await patientResponseService.DeleteAsync(id, responseId);
    }
}