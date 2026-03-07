using System.Security.Claims;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class PatientController(IPatientService patientService) : ControllerBase
{
    [HttpGet]
    [Authorize]
    public Task<IActionResult> GetAllPatientData()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = patientService.GetAllPatientDataService(id);
        return result;
    }

    [HttpPut]
    [Authorize]
    public Task<IActionResult> UpdatePatientData()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
    
}
