using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models;
using Domain.Models.Auth;

namespace Application.Dto
{
    public record UpdatePatientDto(
        string? ImagePath,
        string? UserName,
        string? Email,
        string? PhoneNumber,
        Gender Gender,
        DateOnly? DateOfBirth,
        BloodType? BloodType,
        MedicalRecord? MedicalRecord);
}
