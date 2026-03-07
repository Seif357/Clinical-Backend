using System.Security.Claims;
using Application.Dto;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[ApiController]
[Route("api/[controller]")]
public class PatientController(IPatientService patientService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllPatientData()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await patientService.GetPatientDataServiceAsync(id);
        return result;
    }

    [HttpPut]
    [Authorize]
    public async Task<IActionResult> UpdatePatientData(UpdatePatientDto  updatePatientDto)
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await patientService.UpdatePatientDataServiceAsync(id, updatePatientDto);
        return result;
    }
}
