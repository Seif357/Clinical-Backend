using Application.Dto.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IPatientResponseService
{
    Task<IActionResult> CreateAsync(string patientId, CreatePatientResponseDto dto);
    Task<IActionResult> UpdateAsync(string patientId, int responseId, UpdatePatientResponseDto dto);
    Task<IActionResult> DeleteAsync(string patientId, int responseId);
}