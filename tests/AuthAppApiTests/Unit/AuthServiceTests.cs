using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Application.Dtos;
using AuthApp.Api.Application.Services;
using AuthApp.Api.Domain.Entities;

namespace AuthApp.Api.Tests.Unit;

public class AuthServiceTests
{
    private const string TestPassword = "Password123!";
    private readonly string _testHash;

    public AuthServiceTests()
    {
        var hasher = new PasswordHasher();
        _testHash = hasher.Hash(TestPassword);
    }

    private IUserRepository CreateRepository(User? user = null)
    {
        return new FakeUserRepository(user);
    }

    private IAuthService CreateService(User? user = null)
    {
        return new AuthService(CreateRepository(user), new PasswordHasher(), new FakeTokenService());
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        var user = new User
        {
            Id = 1,
            Email = "demo@example.com",
            PasswordHash = _testHash,
            FullName = "Demo",
            Role = "user",
        };

        var service = CreateService(user);
        var result = await service.LoginAsync(new LoginRequest { Email = "demo@example.com", Password = TestPassword });

        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.NotNull(result.Token);
        Assert.Null(result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ReturnsFail()
    {
        var user = new User
        {
            Id = 1,
            Email = "demo@example.com",
            PasswordHash = _testHash,
            FullName = "Demo",
            Role = "user",
        };

        var service = CreateService(user);
        var result = await service.LoginAsync(new LoginRequest { Email = "demo@example.com", Password = "wrong" });

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Null(result.Token);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ReturnsFail()
    {
        var service = CreateService(user: null);
        var result = await service.LoginAsync(new LoginRequest { Email = "nobody@example.com", Password = TestPassword });

        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Null(result.Token);
        Assert.NotNull(result.Error);
    }

    private sealed class FakeUserRepository(User? user) : IUserRepository
    {
        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            if (user is null || user.Email != email)
            {
                return Task.FromResult<User?>(null);
            }
            return Task.FromResult<User?>(user);
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GenerateToken(User user) => "fake-jwt-token";
    }
}
