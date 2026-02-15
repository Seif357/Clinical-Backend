using Application.Dto.AuthDto;
using Domain.Models.Auth;

namespace Application.Mapper;

public static class RegisterMapper
{
    public static AppUser ToEntity(this RegisterDto dto)
    {
        return new AppUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber
        };
    }
}