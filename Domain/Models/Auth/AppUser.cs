using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Auth
{
    public class AppUser: IdentityUser<int>
    { 
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public ulong RowVersion { get; set; }
        public List<RefreshToken> RefreshTokens { get; set; } = new();


    }
}
