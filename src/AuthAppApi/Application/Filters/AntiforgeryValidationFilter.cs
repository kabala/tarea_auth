using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;

namespace AuthApp.Api.Application.Filters;

public class AntiforgeryValidationFilter : IEndpointFilter
{
    private static readonly HashSet<string> UnsafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST", "PUT", "PATCH", "DELETE",
    };

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        if (UnsafeMethods.Contains(httpContext.Request.Method))
        {
            var antiforgery = httpContext.RequestServices.GetRequiredService<IAntiforgery>();
            var isValid = await antiforgery.IsRequestValidAsync(httpContext);

            if (!isValid)
            {
                return Results.BadRequest(new { message = "Token antiforgery inválido o ausente." });
            }
        }

        return await next(context);
    }
}
