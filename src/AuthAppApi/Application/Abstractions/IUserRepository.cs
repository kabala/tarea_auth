using AuthApp.Api.Domain.Entities;

namespace AuthApp.Api.Application.Abstractions;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
}
