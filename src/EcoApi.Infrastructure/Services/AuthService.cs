using EcoApi.Application.Common.Interfaces;
using EcoApi.Application.Common.Exceptions;
using EcoApi.Application.DTOs.Auth;
using EcoApi.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EcoApi.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuthAuditRepository _authAuditRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuthAuditRepository authAuditRepository,
        IPasswordHasher passwordHasher,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _authAuditRepository = authAuditRepository;
        _passwordHasher = passwordHasher;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var clientIp = GetClientIp();

        if (await _userRepository.GetByEmailAsync(request.Email) != null)
        {
            await WriteAuditAsync("register", null, request.Email, clientIp, isSuccess: false, "duplicate-email");
            throw new DuplicateUserException(request.Email);
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            RegistrationIp = clientIp
        };

        await _userRepository.AddAsync(user);
        await WriteAuditAsync("register", user.Id, user.Email, clientIp, isSuccess: true, null);

        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var clientIp = GetClientIp();
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            await WriteAuditAsync("login", user?.Id, request.Email, clientIp, isSuccess: false, "invalid-credentials");
            throw new InvalidCredentialsException();
        }

        await WriteAuditAsync("login", user.Id, user.Email, clientIp, isSuccess: true, null);
        return await CreateAuthResponseAsync(user);
    }

    public async Task<AuthResponse> RefreshAsync(RefreshTokenRequest request)
    {
        var clientIp = GetClientIp();
        var incomingTokenHash = ComputeTokenHash(request.RefreshToken);
        var storedToken = await _refreshTokenRepository.GetActiveByTokenHashAsync(incomingTokenHash);

        if (storedToken == null)
        {
            await WriteAuditAsync("refresh", null, null, clientIp, isSuccess: false, "invalid-refresh-token");
            throw new InvalidCredentialsException();
        }

        var replacementToken = CreateRefreshToken(storedToken.UserId);
        await _refreshTokenRepository.AddAsync(replacementToken.Entity);

        storedToken.RevokedAt = DateTime.UtcNow;
        storedToken.ReplacedByTokenId = replacementToken.Entity.Id;
        await _refreshTokenRepository.UpdateAsync(storedToken);

        var accessToken = GenerateJwtToken(storedToken.User);
        await WriteAuditAsync("refresh", storedToken.User.Id, storedToken.User.Email, clientIp, isSuccess: true, null);
        return new AuthResponse(accessToken, replacementToken.PlainToken, storedToken.User.Username, storedToken.User.Email);
    }

    private string GenerateJwtToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiryMinutes"]!)),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"]
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<AuthResponse> CreateAuthResponseAsync(User user)
    {
        var accessToken = GenerateJwtToken(user);
        var refreshToken = CreateRefreshToken(user.Id);
        await _refreshTokenRepository.AddAsync(refreshToken.Entity);

        return new AuthResponse(accessToken, refreshToken.PlainToken, user.Username, user.Email);
    }

    private IssuedRefreshToken CreateRefreshToken(int userId)
    {
        var refreshTokenValue = GenerateRefreshTokenValue();
        var refreshTokenExpiryDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var days) ? days : 7;

        var entity = new RefreshToken
        {
            UserId = userId,
            TokenHash = ComputeTokenHash(refreshTokenValue),
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpiryDays)
        };

        return new IssuedRefreshToken(entity, refreshTokenValue);
    }

    private static string GenerateRefreshTokenValue()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeTokenHash(string tokenValue)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenValue));
        return Convert.ToHexString(bytes);
    }

    private string GetClientIp()
    {
        return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "unknown-client";
    }

    private Task WriteAuditAsync(string eventType, int? userId, string? email, string ipAddress, bool isSuccess, string? failureReason)
    {
        var auditLog = new AuthAuditLog
        {
            EventType = eventType,
            UserId = userId,
            Email = email,
            IpAddress = ipAddress,
            IsSuccess = isSuccess,
            FailureReason = failureReason
        };

        return _authAuditRepository.AddAsync(auditLog);
    }

    private sealed record IssuedRefreshToken(RefreshToken Entity, string PlainToken);
}
