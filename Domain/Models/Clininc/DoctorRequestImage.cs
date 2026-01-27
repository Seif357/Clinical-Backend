using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Clininc
{
    public class DoctorRequestImage
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string ImageUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        // Navigation
        public DoctorRequest Request { get; set; } = null!;
    }
}
