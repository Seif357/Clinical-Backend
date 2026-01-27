using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models.Auth
{
    public class AppUser : IdentityUser<int>
    {
        public override string UserName { get; set; }
        public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
        public string ImageUrl { get; set; }
        public ulong RowVersion { get; set; }
        public bool IsBanned { get; set; } = false;
    }
}
