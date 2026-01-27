using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Domain.Models.Clininc
{
    public class Patient_Response
    {
        public string ResponseText { get; set; }
        public string ImageUrl { get; set; }
    }
}
