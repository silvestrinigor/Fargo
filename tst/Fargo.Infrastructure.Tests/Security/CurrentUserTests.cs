using Fargo.Application.Security;
using Fargo.Infrastructure.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.JsonWebTokens;
using System.Security.Claims;

namespace Fargo.Infrastructure.Tests.Security;

public sealed class CurrentUserTests
{
    [Fact]
    public void IsAuthenticated_Should_ReturnFalse_When_HttpContextIsNull()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = null
        };

        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_Should_ReturnFalse_When_UserIsNotAuthenticated()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity()
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.IsAuthenticated;

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsAuthenticated_Should_ReturnTrue_When_UserIsAuthenticated()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.IsAuthenticated;

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void UserGuid_Should_ReturnEmpty_When_HttpContextIsNull()
    {
        // Arrange
        var httpContextAccessor = new HttpContextAccessor
        {
            HttpContext = null
        };

        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void UserGuid_Should_ReturnEmpty_When_UserIsNotAuthenticated()
    {
        // Arrange
        var userGuid = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userGuid.ToString())
                ])
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void UserGuid_Should_ReturnGuidFromNameIdentifier_When_ClaimIsPresent()
    {
        // Arrange
        var userGuid = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, userGuid.ToString())
                ],
                authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(userGuid, result);
    }

    [Fact]
    public void UserGuid_Should_ReturnGuidFromSub_When_NameIdentifierIsMissing()
    {
        // Arrange
        var userGuid = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, userGuid.ToString())
                ],
                authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(userGuid, result);
    }

    [Fact]
    public void UserGuid_Should_PrioritizeNameIdentifier_OverSub()
    {
        // Arrange
        var nameIdentifierGuid = Guid.NewGuid();
        var subGuid = Guid.NewGuid();

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, nameIdentifierGuid.ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, subGuid.ToString())
                ],
                authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(nameIdentifierGuid, result);
    }

    [Fact]
    public void UserGuid_Should_ReturnEmpty_When_NameIdentifierIsInvalid()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, "invalid-guid")
                ],
                authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void UserGuid_Should_ReturnEmpty_When_NoIdentifierClaimsExist()
    {
        // Arrange
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Name, "Igor")
                ],
                authenticationType: "TestAuthType")
        );

        var httpContextAccessor = CreateHttpContextAccessor(principal);
        var sut = new CurrentUser(httpContextAccessor);

        // Act
        var result = sut.UserGuid;

        // Assert
        Assert.Equal(Guid.Empty, result);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(ClaimsPrincipal principal)
    {
        var httpContext = new DefaultHttpContext
        {
            User = principal
        };

        return new HttpContextAccessor
        {
            HttpContext = httpContext
        };
    }
}