using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace AuthApp.Api.Endpoints;

public static class AntiforgeryEndpoints
{
    public const string XsrfCookieName = "XSRF-TOKEN";
    public const string XsrfHeaderName = "X-XSRF-TOKEN";

    public static IEndpointRouteBuilder MapAntiforgeryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/antiforgery/token", (IAntiforgery antiforgery, HttpContext ctx) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(ctx);

            ctx.Response.Cookies.Append(XsrfCookieName, tokens.RequestToken!, new CookieOptions
            {
                HttpOnly = false,
                SameSite = SameSiteMode.Lax,
                Secure = ctx.Request.IsHttps,
                Path = "/",
            });

            return Results.Ok(new { token = tokens.RequestToken });
        })
        .WithTags("Antiforgery");

        return app;
    }
}
