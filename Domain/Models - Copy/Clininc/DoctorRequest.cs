using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class Doctor_Request
    {
        public string RequestType { get; set; }
        // Types: "AnalysisRequest", "ImageRequest", "ReportRequest", "Prescription", "GeneralRequest"
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
    }
}
