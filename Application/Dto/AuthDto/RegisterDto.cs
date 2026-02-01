using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto.AuthDto
{
    public record RegisterDto
   (
 string Username,
 string Email,
 string PhoneNumber,
 string Password,
 string ConfirmPassword
);
}
