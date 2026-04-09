using Application.Dto;
using Domain.Models;
using Domain.Models.Auth;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper
{
    public static class UserMapper
    {
        public static UserDto ToDto(this AppUser user) => new UserDto(
            user.UserName,
            user.Email,
            user.PhoneNumber,
            user.IsDeleted
        );
        public static AppUser ToEntity(this UserDto dto)
        {
            return new AppUser()
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                IsDeleted = dto.IsDeleted
            };
        }
        public static AppUser ToCreateEntity(this CreateUserDto dto)
        {
            return new AppUser()
            {
                UserName = dto.UserName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
            };
        }

        public static void ToUpdateEntity(this UpdatePatientDto dto, AppUser user)
        {
            if (dto.UserName != null)
                user.UserName = dto.UserName;
            if (dto.Email != null)
                user.Email = dto.Email;
            if (dto.PhoneNumber != null)
                user.PhoneNumber = dto.PhoneNumber;
            if (dto.IsDeleted != user.IsDeleted)
                user.IsDeleted = dto.IsDeleted;
        }
    }
}