using Domain.Models.Auth;
using Domain.Models.Clininc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data.Configurations
{
    public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
    {
        public void Configure (EntityTypeBuilder<DoctorProfile> builder)
        {
        }
    }
}
