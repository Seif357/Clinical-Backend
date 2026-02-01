using Application.Dto;
using Application.Dto.AuthDto;
using Domain.Models;
using Domain.Models.Auth;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper
{
    public static class RegisterMapper
    {
        public static AppUser ToEntity(this RegisterDto dto)
        {
            return new AppUser()
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
            };
        }
    }
}