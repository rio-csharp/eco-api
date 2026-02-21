using EcoApi.Application.Common.Interfaces;
using EcoApi.Application.Common.Exceptions;
using EcoApi.Application.DTOs.Auth;
using EcoApi.Domain.Entities;
using EcoApi.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NSubstitute;

namespace EcoApi.Infrastructure.UnitTests;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        var repo = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        repo.GetByEmailAsync(Arg.Any<string>()).Returns(Task.FromResult<User?>(null));
        hasher.Hash(Arg.Any<string>()).Returns("hashed-password");

        var inMemorySettings = new Dictionary<string, string?>{
            {"JwtSettings:Secret","test-secret-which-is-long-enough-1234567"},
            {"JwtSettings:Issuer","EcoApi"},
            {"JwtSettings:Audience","EcoApiUsers"},
            {"JwtSettings:ExpiryMinutes","60"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new AuthService(repo, hasher, config);

        var result = await svc.RegisterAsync(new RegisterRequest("user", "user@example.com", "Password123"));

        result.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be("user@example.com");
        await repo.Received(1).AddAsync(Arg.Any<User>());
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        var repo = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        var password = "Secret123";
        var passHash = "hashed-password";
        var user = new User { Username = "user", Email = "user@example.com", PasswordHash = passHash };
        repo.GetByEmailAsync("user@example.com").Returns(Task.FromResult<User?>(user));
        hasher.Verify(password, passHash).Returns(true);

        var inMemorySettings = new Dictionary<string, string?>{
            {"JwtSettings:Secret","test-secret-which-is-long-enough-1234567"},
            {"JwtSettings:Issuer","EcoApi"},
            {"JwtSettings:Audience","EcoApiUsers"},
            {"JwtSettings:ExpiryMinutes","60"}
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();

        var svc = new AuthService(repo, hasher, config);

        var result = await svc.LoginAsync(new LoginRequest("user@example.com", password));

        result.Token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginAsync_InvalidCredentials_Throws()
    {
        var repo = Substitute.For<IUserRepository>();
        var hasher = Substitute.For<IPasswordHasher>();
        repo.GetByEmailAsync("x@x.com").Returns(Task.FromResult<User?>(null));
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { { "JwtSettings:Secret", "s" } }).Build();
        var svc = new AuthService(repo, hasher, config);
        await Assert.ThrowsAsync<InvalidCredentialsException>(() => svc.LoginAsync(new LoginRequest("x@x.com", "p")));
    }
}
