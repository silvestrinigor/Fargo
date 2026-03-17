using Fargo.Application.Models.AuthModels;
using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class JwtTokenGeneratorTests
{
    private readonly JwtOptions options = new()
    {
        Issuer = "FargoIssuer",
        Audience = "FargoAudience",
        Key = "super-secret-test-key-with-enough-length-12345",
        AccessTokenExpirationInMinutes = 60
    };

    [Fact]
    public void Generate_Should_ReturnTokenGenerateResult()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);

        // Assert
        Assert.IsType<TokenGenerateResult>(result);
    }

    [Fact]
    public void Generate_Should_ReturnNonEmptyToken()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken.Value));
    }

    [Fact]
    public void Generate_Should_SetExpectedExpiration()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();
        var before = DateTimeOffset.UtcNow;

        // Act
        var result = sut.Generate(user);

        var after = DateTimeOffset.UtcNow;

        // Assert
        var expectedMin = before.AddMinutes(options.AccessTokenExpirationInMinutes);
        var expectedMax = after.AddMinutes(options.AccessTokenExpirationInMinutes);

        Assert.True(result.ExpiresAt >= expectedMin);
        Assert.True(result.ExpiresAt <= expectedMax);
    }

    [Fact]
    public void Generate_Should_CreateTokenWithExpectedIssuerAndAudience()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);
        var jwt = ReadJwt(result.AccessToken.Value);

        // Assert
        Assert.Equal(options.Issuer, jwt.Issuer);
        Assert.Contains(options.Audience, jwt.Audiences);
    }

    [Fact]
    public void Generate_Should_CreateTokenWithExpectedNameIdentifierClaim()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);
        var jwt = ReadJwt(result.AccessToken.Value);

        // Assert
        Assert.Equal(
            user.Guid.ToString(),
            jwt.Claims.First(x => x.Type == ClaimTypes.NameIdentifier).Value);
    }

    [Fact]
    public void Generate_Should_CreateTokenWithExpectedSubClaim()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);
        var jwt = ReadJwt(result.AccessToken.Value);

        // Assert
        Assert.Equal(
            user.Guid.ToString(),
            jwt.Claims.First(x => x.Type == JwtRegisteredClaimNames.Sub).Value);
    }

    [Fact]
    public void Generate_Should_CreateTokenWithExpectedNameClaim()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);
        var jwt = ReadJwt(result.AccessToken.Value);

        // Assert
        Assert.Equal(
            user.Nameid.ToString(),
            jwt.Claims.First(x => x.Type == ClaimTypes.Name).Value);
    }

    [Fact]
    public void Generate_Should_CreateTokenSignedWithHmacSha256()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();

        // Act
        var result = sut.Generate(user);
        var jwt = ReadJwt(result.AccessToken.Value);

        // Assert
        Assert.Equal(SecurityAlgorithms.HmacSha256, jwt.SignatureAlgorithm);
    }

    [Fact]
    public void Generate_Should_CreateTokenThatCanBeValidated()
    {
        // Arrange
        var user = CreateUser();
        var sut = CreateSut();
        var tokenHandler = new JwtSecurityTokenHandler();

        // Act
        var result = sut.Generate(user);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = options.Issuer,
            ValidateAudience = true,
            ValidAudience = options.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        var principal = tokenHandler.ValidateToken(
            result.AccessToken.Value,
            validationParameters,
            out var validatedToken);

        // Assert
        Assert.NotNull(principal);
        Assert.IsType<JwtSecurityToken>(validatedToken);
    }

    private JwtTokenGenerator CreateSut()
    {
        return new JwtTokenGenerator(Options.Create(options));
    }

    private static JwtSecurityToken ReadJwt(string token)
    {
        return new JwtSecurityTokenHandler().ReadJwtToken(token);
    }

    private static User CreateUser()
    {
        return new User
        {
            Guid = Guid.NewGuid(),
            Nameid = new Nameid("igor"),
            PasswordHash = new PasswordHash("paswordhashflasfasfjsadfsdfjsfhsajfhsadjfhsadjfhsajdfhiuewhfihfiwuehfewifuhfewiufweiwufhefaf")
        };
    }
}
