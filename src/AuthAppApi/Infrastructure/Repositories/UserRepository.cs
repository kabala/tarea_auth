using AuthApp.Api.Application.Abstractions;
using AuthApp.Api.Domain.Entities;
using AuthApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthApp.Api.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        return db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
