namespace EcoApi.Application.DTOs.Auth;

public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record AuthResponse(string AccessToken, string RefreshToken, string Username, string Email);
