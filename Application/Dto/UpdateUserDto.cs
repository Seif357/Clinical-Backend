using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto
{
    public record UpdateUserDto(
    int Id,
    string? ImagePath,
    string? UserName,
    string? Email,
    string? PhoneNumber,
    DateTime? PremiumEndDate,
    bool IsDeleted
);
}
