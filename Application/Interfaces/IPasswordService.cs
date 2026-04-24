using Application.Dto.AuthDto;
using Application.DTOs;

namespace Application.Interfaces;

public interface IPasswordService
{
    Task<Result> RequestPasswordChangeOtpAsync(int userId, RequestPasswordChangeOtpDto dto);
    Task<Result> ConfirmPasswordChangeAsync(int userId, ConfirmPasswordChangeDto dto);
    Task<Result> ForgotPasswordRequestAsync(ForgotPasswordRequestDto dto);
    Task<Result> ForgotPasswordResetAsync(ForgotPasswordResetDto dto);
}