using AuthApp.Api.Domain.Entities;

namespace AuthApp.Api.Application.Abstractions;

public interface ITokenService
{
    string GenerateToken(User user);
}
