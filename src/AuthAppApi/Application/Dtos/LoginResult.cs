using AuthApp.Api.Domain.Entities;

namespace AuthApp.Api.Application.Dtos;

public record LoginResult(bool Success, User? User, string? Token, string? Error)
{
    public static LoginResult Ok(User user, string token) => new(true, user, token, null);
    public static LoginResult Fail(string error) => new(false, null, null, error);
}
