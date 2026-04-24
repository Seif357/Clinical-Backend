using Application.Dto.AuthDto;
using Application.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<Result> RegisterServiceAsync(RegisterDto registerDto);
    Task<AuthResult> LoginServiceAsync(LoginDto loginDto);
    Task<AuthResult> UpdateEmailServiceAsync(string userId, UpdateEmailDto updateEmailDto);
    Task<AuthResult> UpdateUsernameServiceAsync(string userId, UpdateUsernameDto updateUsernameDto);
    Task<AuthResult> RefreshTokenAsync(string refreshToken); 
    Task<bool> RevokeTokenServiceAsync(string refreshToken, string? revokeReason = null);
    Task<AuthResult> GoogleLoginServiceAsync(GoogleLoginDto dto);
    Task<Result> DeleteAccountService(string userId, string? userRole, string password);

}