using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto.AuthDto
{
    public record PatientRegisterDto
   (
 string Username,
 string Email,
 string PhoneNumber,
 string Password,
 string ConfirmPassword
);
}
