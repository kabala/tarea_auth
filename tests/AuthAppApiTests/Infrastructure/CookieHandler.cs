using System.Net;

namespace AuthApp.Api.Tests.Infrastructure;

public class CookieHandler : DelegatingHandler
{
    private readonly CookieContainer _container = new();
    private static readonly Uri BaseUri = new("http://localhost");

    public string? GetCookieValue(string name)
    {
        var cookies = _container.GetCookies(BaseUri);
        return cookies[name]?.Value;
    }

    public void Clear()
    {
        var cookies = _container.GetCookies(BaseUri);
        foreach (System.Net.Cookie cookie in cookies)
        {
            cookie.Expired = true;
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        var uri = request.RequestUri!;
        var cookieHeader = _container.GetCookieHeader(uri);

        if (!string.IsNullOrEmpty(cookieHeader))
        {
            request.Headers.Remove("Cookie");
            request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
        }

        var response = await base.SendAsync(request, ct);

        if (response.Headers.TryGetValues("Set-Cookie", out var setCookies))
        {
            foreach (var cookie in setCookies)
            {
                try
                {
                    _container.SetCookies(uri, cookie);
                }
                catch (CookieException)
                {
                }
            }
        }

        return response;
    }
}
