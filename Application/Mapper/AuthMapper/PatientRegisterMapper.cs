using Application.Dto;
using Application.Dto.AuthDto;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Clininc;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper
{
    public static class PatientRegisterMapper
    {
        public static PatientProfile ToEntity(this PatientRegisterDto dto)
        {
            return new PatientProfile()
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
            };
        }
    }
}