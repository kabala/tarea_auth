using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Application.Dtos;

namespace AuthApp.Api.Application.Services;

public class AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, ITokenService tokenService)
    : IAuthService
{
    public async Task<LoginResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await userRepository.GetByEmailAsync(request.Email, ct);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return LoginResult.Fail("Credenciales inválidas.");
        }

        var token = tokenService.GenerateToken(user);
        return LoginResult.Ok(user, token);
    }
}
