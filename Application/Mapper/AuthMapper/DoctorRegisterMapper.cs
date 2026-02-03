using Application.Dto;
using Application.Dto.AuthDto;
using Domain.Models;
using Domain.Models.Auth;
using Domain.Models.Clininc;
using System.Collections.Generic;
using System.Linq;

namespace Application.Mapper
{
    public static class DoctorRegisterMapper
    {
        public static DoctorProfile ToEntity(this DoctorRegisterDto dto)
        {
            return new DoctorProfile()
            {
                UserName = dto.Username,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                CertificationNumber = dto.certificationNumber
            };
        }
    }
}