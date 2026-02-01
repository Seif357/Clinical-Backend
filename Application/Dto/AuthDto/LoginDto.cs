using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Dto.AuthDto
{
    public record LoginDto
   (
 string Username_EmailOrPhoneNumber,
 string Password
);
}
