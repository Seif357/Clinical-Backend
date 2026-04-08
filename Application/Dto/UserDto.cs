using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto
{
    public record UserDto
       (
     string? UserName,
     string? Email,
     string? PhoneNumber,
     bool IsDeleted
    );
}
