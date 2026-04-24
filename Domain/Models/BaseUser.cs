using Domain.Models.Auth;

namespace Domain.Models;

public class BaseUser
{
    public string? ImagePath { get; set; }
    public Gender Gender { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; }
}