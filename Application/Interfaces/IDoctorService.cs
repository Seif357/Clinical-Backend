using Application.Dto;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IDoctorService
{
    Task<IActionResult> GetDoctorDataServiceAsync(string userId);
    Task<IActionResult> UpdateDoctorDataServiceAsync(string userId, UpdateDoctorDto updateDoctorDto);
}