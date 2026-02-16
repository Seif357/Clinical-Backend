using Domain.Models;
using Domain.Models.AI;
using Domain.Models.Auth;
using Domain.Models.Communication;
using Domain.Models.MedicalRecordAttributes;
using Domain.Models.Schedule;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataAccess;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<int>, int>(options)
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<MedicalRecord> MedicalRecords { get; set; }
    public DbSet<Allergy> Allergies { get; set; }
    public DbSet<FamilyCondition> FamilyConditions { get; set; }
    public DbSet<PrescribedMedication> PrescribedMedications { get; set; }
    public DbSet<Surgery> Surgeries { get; set; }
    public DbSet<TestTaken> TestsTaken { get; set; }
    public DbSet<Visit> Visits { get; set; }
    public DbSet<DoctorRequest> DoctorRequests { get; set; }
    public DbSet<DoctorResponse> DoctorResponses { get; set; }
    public DbSet<DoctorReqestImage> DoctorReqestImages { get; set; }
    public DbSet<PatientRequest> PatientRequests { get; set; }
    public DbSet<PatientResponse> PatientResponses { get; set; }
    public DbSet<PatientRequestImage> PatientRequestImages { get; set; }
    public DbSet<PatientResponseImage> PatientResponseImages { get; set; }
    public DbSet<ModelInput> ModelInputs { get; set; }
    public DbSet<ModelOutput> ModelOutputs { get; set; }
    public DbSet<Schedule> Schedules { get; set; }
    public DbSet<ScheduleSlot> ScheduleSlots { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.Entity<Patient>()
            .Property(p => p.UserId)
            .ValueGeneratedNever();
        builder.Entity<Doctor>()
            .Property(p => p.UserId)
            .ValueGeneratedNever();
        builder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Configure ModelInput and ModelOutput relationship
        builder.Entity<ModelOutput>()
            .HasOne(mo => mo.Input)
            .WithOne(mi => mi.Output)
            .HasForeignKey<ModelOutput>(mo => mo.ModelInputId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Add indexes for better query performance
        builder.Entity<ModelInput>()
            .HasIndex(mi => mi.PatientId);
        
        builder.Entity<ModelInput>()
            .HasIndex(mi => mi.UploadedAt);
    }
}