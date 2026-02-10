using Domain.Models.Auth;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class BaseUser
    {
        public string ImagePath { get; set; }
        public Gender Gender { get; set; }
    }
}
