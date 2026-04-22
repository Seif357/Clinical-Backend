using Application.Dto.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IDoctorRequestService
{
    Task<IActionResult> GetAllSummaryAsync(string doctorId);
    Task<IActionResult> GetByIdAsync(string doctorId, int requestId);
    Task<IActionResult> CreateAsync(string doctorId, CreateDoctorRequestDto dto);
    Task<IActionResult> UpdateAsync(string doctorId, int requestId, UpdateDoctorRequestDto dto);
    Task<IActionResult> DeleteAsync(string doctorId, int requestId);
}
