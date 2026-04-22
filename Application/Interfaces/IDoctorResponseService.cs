using Application.Dto.Communication;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IDoctorResponseService
{
    Task<IActionResult> CreateAsync(string doctorId, CreateDoctorResponseDto dto);
    Task<IActionResult> UpdateAsync(string doctorId, int responseId, UpdateDoctorResponseDto dto);
    Task<IActionResult> DeleteAsync(string doctorId, int responseId);
}
