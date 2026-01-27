using Domain.Models.Clininc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    internal class PatientResponseConfiguration : IEntityTypeConfiguration<PatientResponse>
    {
        public void Configure(EntityTypeBuilder<PatientResponse> builder)
        {
            builder.HasIndex(e => e.DoctorRequestId);
            builder.HasIndex(e => new { e.SubmittedByUserId, e.SubmittedAt });

            builder.HasOne(r => r.SubmittedByUser)
                .WithMany()
                .HasForeignKey(r => r.SubmittedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}