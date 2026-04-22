using Application.Dto.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IPatientRequestService
{
    Task<IActionResult> GetAllSummaryAsync(string patientId);
    Task<IActionResult> GetByIdAsync(string patientId, int requestId);
    Task<IActionResult> CreateAsync(string patientId, CreatePatientRequestDto dto);
    Task<IActionResult> UpdateAsync(string patientId, int requestId, UpdatePatientRequestDto dto);
    Task<IActionResult> DeleteAsync(string patientId, int requestId);
}
