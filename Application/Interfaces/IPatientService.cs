using Application.Dto;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IPatientService
{
    Task<IActionResult> GetPatientDataServiceAsync(string id);
    Task<IActionResult> UpdatePatientDataServiceAsync(string id, UpdatePatientDto updatePatientDto);
}