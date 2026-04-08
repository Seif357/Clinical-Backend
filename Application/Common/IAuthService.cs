using Application.Dto;
using Application.Dto.AuthDto;
using Application.DTOs;

namespace Application.Common.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterServiceAsync(RegisterDto registerDto);
    Task<AuthResult> LoginServiceAsync(LoginDto loginDto);
    Task<AuthResult> UpdateEmailServiceAsync(string userId, UpdateEmailDto updateEmailDto);
    Task<AuthResult> UpdateUsernameServiceAsync(string userId, UpdateUsernameDto updateUsernameDto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken);
    Task<AuthResult> UpdatePasswordServiceAsync(string userId,UpdatePasswordDto updateUserDto);
    Task<bool> RevokeTokenServiceAsync(string refreshToken, string? revokeReason = null);



}
