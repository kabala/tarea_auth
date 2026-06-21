using AuthApp.Api.Application.Dtos;

namespace AuthApp.Api.Application.Abstractions;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default);
}
