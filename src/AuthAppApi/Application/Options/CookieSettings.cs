namespace AuthApp.Api.Application.Options;

public class CookieSettings
{
    public const string SectionName = "Cookie";
    public const string AuthCookieName = "auth_app_session";

    public bool Secure { get; set; } = true;
    public string SameSite { get; set; } = "Lax";
}
