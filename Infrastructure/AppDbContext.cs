using Domain.Models.Auth;
using Domain.Models.Clininc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Configurations;


namespace Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, IdentityRole<int>, int>(options)
    {
        public DbSet<AppUser> User { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<AppointmentRequest> AppointmentRequests { get; set; }
        public DbSet<DoctorProfile>  DoctorProfiles { get; set; }
        public DbSet<DoctorRequestImage> DoctorRequestImages { get; set; }
        public DbSet<PatientProfile> PatientProfiles { get; set; }
        public DbSet<PatientResponse>  PatientResponses { get; set; }
        public DbSet<PatientResponseImage> PatientResponseImages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
