using Domain.Models.Auth;

namespace Domain.Models;

public class BaseUser
{
    public string? ImagePath { get; set; }
    public Gender Gender { get; set; }
}