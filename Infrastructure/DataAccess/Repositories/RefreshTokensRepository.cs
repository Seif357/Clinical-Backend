using Domain.Models;
using Domain.Models.Auth;

namespace Infrastructure.DataAccess.Repositories;

public class RefreshTokensRepository(AppDbContext context) : GenericRepository<RefreshToken>(context), IRefreshTokenRepository;