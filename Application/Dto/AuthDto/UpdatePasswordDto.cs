using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto.AuthDto
{
    public record UpdatePasswordDto
   (
        string Password,
 string NewPassword,
 string ConfirmNewPassword
);
}
