using Application.Dto;
using Domain.Models;

namespace Application.Mapper;

public static class PatientMapper
{
    public static PatientDto ToDto(this Patient p) => new(
        UserId:               p.UserId,
        UserName:             p.PatientData.UserName ?? string.Empty,
        Email:                p.PatientData.Email    ?? string.Empty,
        PhoneNumber:          p.PatientData.PhoneNumber,
        EmailConfirmed:       p.PatientData.EmailConfirmed,
        PhoneNumberConfirmed: p.PatientData.PhoneNumberConfirmed,
        ImagePath:            p.ImagePath,
        Gender:               p.Gender,
        BloodType:            p.BloodType,
        DateOfBirth:          p.DateOfBirth,
        CreatedAt:            p.CreatedAt,
        UpdatedAt:            p.UpdatedAt
        );

}