using EcoApi.Application.Common.Interfaces;
using EcoApi.Application.Common.Exceptions;
using EcoApi.Application.DTOs.Auth;
using EcoApi.Domain.Entities;
using FluentAssertions;
using EcoApi.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using System.Security.Cryptography;
using System.Text;

namespace EcoApi.Infrastructure.UnitTests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        var repo = Substitute.For<IUserRepository>();
        var refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
        var auditRepo = Substitute.For<IAuthAuditRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());
        repo.GetByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null));
        hasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var inMemorySettings = new Dictionary<string, string?>{
            {"JwtSettings:Secret","test-secret-which-is-long-enough-1234567"},
            {"JwtSettings:Issuer","EcoApi"},
            {"JwtSettings:Audience","EcoApiUsers"},
            {"JwtSettings:ExpiryMinutes","60"},
            {"JwtSettings:RefreshTokenExpiryDays","7"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new AuthService(repo, refreshTokenRepo, auditRepo, hasher, httpContextAccessor, config);

        var result = await svc.RegisterAsync(new RegisterRequest("user", "user@example.com", "Password123"));

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("user@example.com");
        await repo.Received(1).AddAsync(Arg.Any<User>());
        await refreshTokenRepo.Received(1).AddAsync(Arg.Any<RefreshToken>());
        await auditRepo.Received(1).AddAsync(Arg.Is<AuthAuditLog>(x => x.EventType == "register" && x.IsSuccess));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var repo = Substitute.For<IUserRepository>();
        var refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
        var auditRepo = Substitute.For<IAuthAuditRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());
        var password = "Secret123";
        var passHash = "hashed-password";
        var user = new User { Username = "user", Email = "user@example.com", PasswordHash = passHash };
        repo.GetByEmailAsync("user@example.com").Returns(Task.FromResult<User?>(user));
        hasher.Verify(password, passHash).Returns(true);

        var inMemorySettings = new Dictionary<string, string?>{
            {"JwtSettings:Secret","test-secret-which-is-long-enough-1234567"},
            {"JwtSettings:Issuer","EcoApi"},
            {"JwtSettings:Audience","EcoApiUsers"},
            {"JwtSettings:ExpiryMinutes","60"},
            {"JwtSettings:RefreshTokenExpiryDays","7"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new AuthService(repo, refreshTokenRepo, auditRepo, hasher, httpContextAccessor, config);

        var result = await svc.LoginAsync(new LoginRequest("user@example.com", password));

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        await auditRepo.Received(1).AddAsync(Arg.Is<AuthAuditLog>(x => x.EventType == "login" && x.IsSuccess));
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_Throws()
    {
        var repo = Substitute.For<IUserRepository>();
        var refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
        var auditRepo = Substitute.For<IAuthAuditRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());
        repo.GetByEmailAsync("x@x.com").Returns(Task.FromResult<User?>(null));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "JwtSettings:Secret", "s" } }).Build();
        var svc = new AuthService(repo, refreshTokenRepo, auditRepo, hasher, httpContextAccessor, config);
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => svc.LoginAsync(new LoginRequest("x@x.com", "p")));
        await auditRepo.Received(1).AddAsync(Arg.Is<AuthAuditLog>(x => x.EventType == "login" && !x.IsSuccess));
    }

    [Fact]
    public async Task RefreshAsync_ValidToken_RotatesAndReturnsNewTokens()
    {
        var repo = Substitute.For<IUserRepository>();
        var refreshTokenRepo = Substitute.For<IRefreshTokenRepository>();
        var auditRepo = Substitute.For<IAuthAuditRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(new DefaultHttpContext());
        var user = new User { Id = 7, Username = "user", Email = "user@example.com", PasswordHash = "hash" };
        var plainRefreshToken = "refresh-token";
        var existingToken = new RefreshToken
        {
            Id = 1,
            UserId = user.Id,
            User = user,
            TokenHash = ComputeTokenHash(plainRefreshToken),
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };

        refreshTokenRepo.GetActiveByTokenHashAsync(existingToken.TokenHash).Returns(existingToken);
        refreshTokenRepo.When(x => x.AddAsync(Arg.Any<RefreshToken>()))
            .Do(callInfo => callInfo.Arg<RefreshToken>().Id = 99);

        var inMemorySettings = new Dictionary<string, string?>{
            {"JwtSettings:Secret","test-secret-which-is-long-enough-1234567"},
            {"JwtSettings:Issuer","EcoApi"},
            {"JwtSettings:Audience","EcoApiUsers"},
            {"JwtSettings:ExpiryMinutes","60"},
            {"JwtSettings:RefreshTokenExpiryDays","7"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new AuthService(repo, refreshTokenRepo, auditRepo, hasher, httpContextAccessor, config);

        var result = await svc.RefreshAsync(new RefreshTokenRequest(plainRefreshToken));

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(plainRefreshToken);
        existingToken.RevokedAt.Should().NotBeNull();
        existingToken.ReplacedByTokenId.Should().Be(99);
        await refreshTokenRepo.Received(1).UpdateAsync(existingToken);
        await auditRepo.Received(1).AddAsync(Arg.Is<AuthAuditLog>(x => x.EventType == "refresh" && x.IsSuccess));
    }

    private static string ComputeTokenHash(string tokenValue)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(tokenValue));
        return Convert.ToHexString(bytes);
    }
}
