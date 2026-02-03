using Domain.Models.Clininc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
    {
        public void Configure(EntityTypeBuilder<PatientProfile> builder)
        {

            builder.HasOne(p => p.Doctor)
            .WithMany(d => d.Patients)
            .HasForeignKey(p => p.DoctorProfileId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
