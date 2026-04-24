using Domain.Models.Auth;

namespace Infrastructure.Repositories.Interfaces;

public interface IOtpRepository
{
    Task AddAsync(OtpRecord record);
    Task InvalidatePreviousAsync(int userId, OtpPurpose purpose);
    Task<OtpRecord?> GetActiveAsync(int userId, OtpPurpose purpose);
    Task MarkUsedAsync(OtpRecord record);
}