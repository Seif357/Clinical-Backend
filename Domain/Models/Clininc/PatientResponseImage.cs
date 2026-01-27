using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Clininc
{
    public class PatientResponseImage
    {
        public int Id { get; set; }
        public int ResponseId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }

        // Navigation
        public PatientResponse Response { get; set; } = null!;
    }
}
