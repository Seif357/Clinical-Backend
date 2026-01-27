using Domain.Models.Clininc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    internal class DoctorRequestConfiguration : IEntityTypeConfiguration<DoctorRequest>
    {
        public void Configure(EntityTypeBuilder<DoctorRequest> builder)
        {
            builder.HasIndex(e => new { e.PatientProfileId, e.CreatedAt });
            builder.HasIndex(e => new { e.RequestType, e.Status });

            builder.HasOne(r => r.PatientProfile)
                .WithMany(p => p.DoctorRequests)
                .HasForeignKey(r => r.PatientProfileId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

            builder.HasOne(r => r.DoctorProfile)
                .WithMany(d => d.DoctorRequests)
                .HasForeignKey(r => r.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}