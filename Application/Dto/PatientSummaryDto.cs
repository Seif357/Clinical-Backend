using Domain.Models;

namespace Application.Dto;

public class PatientSummaryDto
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    public string? BloodType { get; set; }
}
