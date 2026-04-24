using Application.Dto.AuthDto;
using Application.Dto.Email_management;
using Application.Dto.Phone_management;
using Application.DTOs;
 
namespace Application.Interfaces;
 
public interface IContactService
{
    Task<Result> AddEmailAsync(int userId, string displayName, AddEmailDto dto);
    Task<Result> VerifyEmailAsync(int userId, VerifyEmailDto dto);
    Task<Result> ResendEmailVerificationAsync(int userId, ResendEmailVerificationDto dto);
    Task<Result> SetPrimaryEmailAsync(int userId, SetPrimaryEmailDto dto);
    Task<Result> RemoveEmailAsync(int userId, RemoveEmailDto dto);
 
    Task<Result> AddPhoneAsync(int userId, string displayName, AddPhoneDto dto);
    Task<Result> VerifyPhoneAsync(int userId, VerifyPhoneDto dto);
    Task<Result> ResendPhoneVerificationAsync(int userId, ResendPhoneVerificationDto dto);
    Task<Result> SetPrimaryPhoneAsync(int userId, SetPrimaryPhoneDto dto);
    Task<Result> RemovePhoneAsync(int userId, RemovePhoneDto dto);
}