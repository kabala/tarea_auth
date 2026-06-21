using AuthApp.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AuthApp.Api.Tests.Infrastructure;

public class AuthAppWebAppFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public AuthAppWebAppFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var efDescriptors = services
                .Where(d =>
                {
                    var names = new[]
                    {
                        d.ServiceType?.FullName,
                        d.ImplementationType?.FullName,
                        d.ImplementationInstance?.GetType().FullName,
                    };
                    return names.Any(n => n?.Contains("EntityFrameworkCore") == true);
                })
                .ToList();

            foreach (var d in efDescriptors)
            {
                services.Remove(d);
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
        });
    }

    public HttpClient CreateClientWithCookies()
    {
        return CreateDefaultClient(new CookieHandler());
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}
