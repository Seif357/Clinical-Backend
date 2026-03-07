using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IPatientService
{
    Task<IActionResult> GetAllPatientDataService(string id);
}