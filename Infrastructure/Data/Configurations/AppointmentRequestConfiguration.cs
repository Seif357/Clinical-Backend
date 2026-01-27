using Domain.Models.Clininc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    internal class AppointmentRequestConfiguration : IEntityTypeConfiguration<AppointmentRequest>
    {
        public void Configure(EntityTypeBuilder<AppointmentRequest> builder)
        {
            builder.HasIndex(e => new { e.DoctorProfileId, e.Status, e.RequestedAt });

            builder.HasOne(a => a.PatientProfile)
                .WithMany(p => p.AppointmentRequests)
                .HasForeignKey(a => a.PatientProfileId)
                .OnDelete(DeleteBehavior.Restrict); // Changed to Restrict

            builder.HasOne(a => a.DoctorProfile)
                .WithMany(d => d.AppointmentRequests)
                .HasForeignKey(a => a.DoctorProfileId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
