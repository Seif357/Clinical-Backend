using System.Security.Claims;
using Application.Dto.Communication;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DoctorResponseController(IDoctorResponseService doctorResponseService) : ControllerBase
{
    // No GET — responses are fetched via GET /api/PatientRequest/{id}

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDoctorResponseDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorResponseService.CreateAsync(id, dto);
    }

    [HttpPut("{responseId:int}")]
    public async Task<IActionResult> Update(int responseId, [FromBody] UpdateDoctorResponseDto dto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorResponseService.UpdateAsync(id, responseId, dto);
    }

    [HttpDelete("{responseId:int}")]
    public async Task<IActionResult> Delete(int responseId)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return await doctorResponseService.DeleteAsync(id, responseId);
    }
}