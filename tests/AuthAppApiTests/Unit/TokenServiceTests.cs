using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Application.Options;
using AuthApp.Api.Application.Services;
using AuthApp.Api.Domain.Entities;
using Microsoft.Extensions.Options;

namespace AuthApp.Api.Tests.Unit;

public class TokenServiceTests
{
    private readonly JwtOptions _jwtOptions = new()
    {
        SigningKey = "test-signing-key-with-at-least-32-characters!",
        Issuer = "TestIssuer",
        Audience = "TestAudience",
        ExpiryMinutes = 60,
    };

    private ITokenService CreateService()
    {
        return new TokenService(Options.Create(_jwtOptions));
    }

    [Fact]
    public void GenerateToken_ReturnsValidJwt_WithCorrectClaims()
    {
        var service = CreateService();
        var user = new User
        {
            Id = 42,
            Email = "test@example.com",
            FullName = "Test User",
            Role = "admin",
        };

        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("TestIssuer", jwt.Issuer);
        Assert.Equal("TestAudience", jwt.Audiences.First());
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Name && c.Value == "Test User");
        Assert.Contains(jwt.Claims, c => c.Type == ClaimTypes.Role && c.Value == "admin");
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == "42");
        Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
    }

    [Fact]
    public void GenerateToken_SetsExpiration_BasedOnOptions()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "a@b.com", FullName = "A", Role = "user" };

        var token = service.GenerateToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        var expectedExpiry = DateTimeOffset.UtcNow.AddMinutes(60);
        var actualExpiry = new DateTimeOffset(jwt.ValidTo);

        Assert.True(Math.Abs((expectedExpiry - actualExpiry).TotalSeconds) < 5);
    }

    [Fact]
    public void GenerateToken_GeneratesUniqueJti_EachCall()
    {
        var service = CreateService();
        var user = new User { Id = 1, Email = "a@b.com", FullName = "A", Role = "user" };

        var token1 = service.GenerateToken(user);
        var token2 = service.GenerateToken(user);

        var jwt1 = new JwtSecurityTokenHandler().ReadJwtToken(token1);
        var jwt2 = new JwtSecurityTokenHandler().ReadJwtToken(token2);

        var jti1 = jwt1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwt2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        Assert.NotEqual(jti1, jti2);
    }
}
