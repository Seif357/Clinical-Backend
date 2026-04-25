using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces;

public interface IDoctorService
{
    Task<IActionResult> GetDoctorDataServiceAsync(string userId);
    Task<IActionResult> UpdateDoctorDataServiceAsync(string userId, UpdateDoctorDto updateDoctorDto);
    Task<Result> SearchDoctorsAsync(DoctorSearchQuery q);
}