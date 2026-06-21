namespace AuthApp.Api.Application.Dtos;

public record LoginResponse(int Id, string Email, string FullName, string Role);
