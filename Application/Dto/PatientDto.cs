using Application.Dto.MedicalRecord;
using Domain.Models;
using Domain.Models.Auth;

namespace Application.Dto;

public record PatientDto(
    int UserId,
    string UserName,
    string Email,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool PhoneNumberConfirmed,
    string? ImagePath,
    Gender Gender,
    BloodType? BloodType,
    DateOnly? DateOfBirth,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    MedicalRecordDto? MedicalRecord
);