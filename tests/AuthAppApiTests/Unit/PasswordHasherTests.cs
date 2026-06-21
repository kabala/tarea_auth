using AuthApp.Api.Application.Services;

namespace AuthApp.Api.Tests.Unit;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsNonEmptyString_DifferentFromInput()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.False(string.IsNullOrEmpty(hash));
        Assert.NotEqual("Password123!", hash);
    }

    [Fact]
    public void Verify_WithCorrectPassword_ReturnsTrue()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.True(hasher.Verify("Password123!", hash));
    }

    [Fact]
    public void Verify_WithIncorrectPassword_ReturnsFalse()
    {
        var hasher = new PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.False(hasher.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Hash_GeneratesDifferentHashesForSamePassword()
    {
        var hasher = new PasswordHasher();
        var hash1 = hasher.Hash("Password123!");
        var hash2 = hasher.Hash("Password123!");

        Assert.NotEqual(hash1, hash2);
    }
}
