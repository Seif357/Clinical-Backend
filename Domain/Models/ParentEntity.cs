using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Models
{
    public class ParentEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; }
        public ulong RowVersion { get; set; }
        public bool IsDeleted { get; set; }
    }
}
