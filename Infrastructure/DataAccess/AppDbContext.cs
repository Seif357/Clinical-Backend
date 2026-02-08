using Domain.Models;
using Domain.Models.Auth;
using Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Domain.Models.MedicalRecordAttributes;
using Domain.Models.Communication;
using Domain.Models.AI;
using Domain.Models.Schedule;

namespace Infrastructure.DataAccess
{
    public class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<int>, int>(options)
    {
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<FamilyCondition> FamilyConditions { get; set; }
        public DbSet<MedicationTaken> MedicationTakens { get; set; }
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
            }

    }
}
