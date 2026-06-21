using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Application.Dtos;
using AuthApp.Api.Application.Filters;
using AuthApp.Api.Application.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace AuthApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Auth");

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthService authService,
            ITokenService tokenService,
            IOptions<JwtOptions> jwtOptions,
            IOptions<CookieSettings> cookieSettings,
            HttpContext ctx) =>
        {
            var validationErrors = ValidateLoginRequest(request);
            if (validationErrors.Count > 0)
            {
                return Results.BadRequest(new { message = string.Join(" ", validationErrors) });
            }

            var result = await authService.LoginAsync(request, ctx.RequestAborted);

            if (!result.Success || result.User is null || result.Token is null)
            {
                return Results.Json(
                    new { message = result.Error ?? "Credenciales inválidas." },
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            var jwt = jwtOptions.Value;
            var cookie = cookieSettings.Value;
            var sameSite = Enum.Parse<SameSiteMode>(cookie.SameSite, ignoreCase: true);

            ctx.Response.Cookies.Append(CookieSettings.AuthCookieName, result.Token, new CookieOptions
            {
                HttpOnly = true,
                Secure = cookie.Secure,
                SameSite = sameSite,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddMinutes(jwt.ExpiryMinutes),
                Path = "/",
            });

            return Results.Ok(new LoginResponse(result.User.Id, result.User.Email, result.User.FullName, result.User.Role));
        })
        .AddEndpointFilter<AntiforgeryValidationFilter>();

        group.MapPost("/logout", (HttpContext ctx) =>
        {
            ctx.Response.Cookies.Delete(CookieSettings.AuthCookieName, new CookieOptions
            {
                Path = "/",
            });
            return Results.Ok(new { message = "Sesión cerrada correctamente." });
        })
        .RequireAuthorization();

        group.MapGet("/me", (HttpContext ctx) =>
        {
            var user = ctx.User;
            return Results.Ok(new LoginResponse(
                int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!),
                user.FindFirstValue(ClaimTypes.Email)!,
                user.FindFirstValue(ClaimTypes.Name)!,
                user.FindFirstValue(ClaimTypes.Role)!));
        })
        .RequireAuthorization();

        group.MapGet("/admin", (HttpContext ctx) =>
        {
            return Results.Ok(new
            {
                message = $"Bienvenido, {ctx.User.FindFirstValue(ClaimTypes.Name)}. Tienes acceso de administrador.",
            });
        })
        .RequireAuthorization("Admin");

        return app;
    }

    private static List<string> ValidateLoginRequest(LoginRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add("El correo es obligatorio.");
        }
        else if (!new EmailAddressAttribute().IsValid(request.Email))
        {
            errors.Add("El correo no tiene un formato válido.");
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            errors.Add("La contraseña es obligatoria.");
        }
        else if (request.Password.Length < 6)
        {
            errors.Add("La contraseña debe tener al menos 6 caracteres.");
        }

        return errors;
    }
}
