using Domain.Models.Auth;
using FluentAssertions;
using Infrastructure.DataAccess;
using Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Infrastructure.Repositories;

public class RefreshTokenRepositoryTests
{
    // Helper method to create a fresh, isolated InMemory database for each test
    // This ensures tests don't interfere with each other's data
    private AppDbContext GetInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }

    #region AddAsync & UpdateAsync Tests

    [Fact]
    public async Task AddAsync_ValidToken_AddsAndSavesToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        var refreshToken = new RefreshToken
        {
            Token = "test-token-123",
            UserId = 1,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Act
        await sut.AddAsync(refreshToken);

        // Assert
        var savedToken = await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "test-token-123");
        savedToken.Should().NotBeNull();
        savedToken!.UserId.Should().Be(1);
    }

    [Fact]
    public async Task AddAsync_NullToken_ThrowsArgumentNullException()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        // Act
        Func<Task> action = async () => await sut.AddAsync(null!);

        // Assert
        await action.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateAsync_ValidToken_UpdatesAndSavesToDatabase()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        var refreshToken = new RefreshToken
        {
            Token = "update-token",
            UserId = 1,
            ReasonRevoked = null
        };
        await context.RefreshTokens.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        // Act
        refreshToken.ReasonRevoked = "Security Breach";
        await sut.UpdateAsync(refreshToken);

        // Assert
        var updatedToken = await context.RefreshTokens.FirstAsync(rt => rt.Token == "update-token");
        updatedToken.ReasonRevoked.Should().Be("Security Breach");
    }

    #endregion

    #region Retrieval Tests (GetByToken, GetUserToken, GetActiveTokens)

    [Fact]
    public async Task GetByTokenAsync_ExistingToken_ReturnsTokenWithoutUser()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        var tokenString = "existing-token";
        await context.RefreshTokens.AddAsync(new RefreshToken { Token = tokenString, UserId = 1 });
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByTokenAsync(tokenString);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(tokenString);
        // Note: AsNoTracking is used in the repository, so we just verify the properties
    }

    [Fact]
    public async Task GetActiveTokensByUserIdAsync_HasActiveAndInactiveTokens_ReturnsOnlyActive()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);
        var userId = 1;
        var now = DateTime.UtcNow;

        var tokens = new[]
        {
            new RefreshToken { Token = "active-1", UserId = userId, RevokedAt = null, ExpiresAt = now.AddDays(1) },
            new RefreshToken { Token = "expired", UserId = userId, RevokedAt = null, ExpiresAt = now.AddDays(-1) },
            new RefreshToken { Token = "revoked", UserId = userId, RevokedAt = now, ExpiresAt = now.AddDays(1) },
            new RefreshToken { Token = "other-user", UserId = 2, RevokedAt = null, ExpiresAt = now.AddDays(1) }
        };

        await context.RefreshTokens.AddRangeAsync(tokens);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetActiveTokensByUserIdAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(1);
        result.First().Token.Should().Be("active-1");
    }

    #endregion

    #region Validation Tests (IsTokenValidAsync)

    [Fact]
    public async Task IsTokenValidAsync_ValidAndActiveToken_ReturnsTrue()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        var tokenString = "valid-token";

        // Assuming ApplicationUser or similar user model exists
        // Note: Adjust the User object instantiation based on your actual Domain User class
        var token = new RefreshToken
        {
            Token = tokenString,
            RevokedAt = null,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            User = new AppUser { Id = 1, UserName = "TestUser" }
        };

        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();

        // Act
        var isValid = await sut.IsTokenValidAsync(tokenString);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task IsTokenValidAsync_RevokedToken_ReturnsFalse()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);

        var tokenString = "revoked-token";
        var token = new RefreshToken
        {
            Token = tokenString,
            RevokedAt = DateTime.UtcNow, // It is revoked
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            User = new AppUser { Id = 1 }
        };

        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();

        // Act
        var isValid = await sut.IsTokenValidAsync(tokenString);

        // Assert
        isValid.Should().BeFalse();
    }

    #endregion

    #region Revoke Tests (ExecuteUpdateAsync)

    [Fact]
    public async Task RevokeAllUserTokensAsync_ActiveTokens_RevokesThemSuccessfully()
    {
        // Arrange
        var context = GetInMemoryDbContext();
        var sut = new RefreshTokenRepository(context);
        var userId = 5;
        var reason = "User logged out";
        var now = DateTime.UtcNow;

        var dummyUser = new AppUser { Id = userId, UserName = "testuser", Email = "test@test.com" };

        await context.Set<AppUser>().AddAsync(dummyUser);

        var token = new RefreshToken
        {
            Token = "to-be-revoked",
            UserId = userId,
            RevokedAt = null,
            ExpiresAt = now.AddDays(1)
        };

        await context.RefreshTokens.AddAsync(token);
        await context.SaveChangesAsync();

        // Act
        await sut.RevokeAllUserTokensAsync(userId, reason);

        // Assert
        var revokedToken = await context.RefreshTokens.AsNoTracking().FirstAsync(rt => rt.Token == "to-be-revoked");
        revokedToken.RevokedAt.Should().NotBeNull();
        revokedToken.ReasonRevoked.Should().Be(reason);
    }

    #endregion
}