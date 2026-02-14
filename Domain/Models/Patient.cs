using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Models.Auth;

namespace Domain.Models;

public class Patient : BaseUser
{
    [Key] public int UserId { get; set; }

    public DateOnly? DateOfBirth { get; set; }
    public BloodType? BloodType { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }

    [ForeignKey("UserId")] public AppUser PatientData { get; set; }
}