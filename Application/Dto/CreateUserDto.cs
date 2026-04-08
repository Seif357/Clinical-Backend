using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto
{
    public record CreateUserDto(
     string UserName,
     string Email,
     string Password,
     string? PhoneNumber = null
 );
}
