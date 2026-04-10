using Fargo.Sdk.Authentication;
using Fargo.Sdk.Http;
using NSubstitute;
using System.Net;

namespace Fargo.Sdk.Tests.Authentication;

public sealed class AuthenticationClientTests
{
    private readonly IFargoSdkHttpClient httpClient = Substitute.For<IFargoSdkHttpClient>();
    private readonly AuthenticationClient sut;

    public AuthenticationClientTests()
    {
        sut = new AuthenticationClient(httpClient);
    }

    // --- LogInAsync ---

    [Fact]
    public async Task LogInAsync_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(true, Fakes.AuthResult(), null, HttpStatusCode.OK));

        // Act
        var result = await sut.LogInAsync("user", "pass");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task LogInAsync_Should_ReturnInvalidCredentials_When_ProblemTypeIsInvalidPassword()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, Fakes.Problem("auth/invalid-password"), HttpStatusCode.BadRequest));

        // Act
        var result = await sut.LogInAsync("user", "wrong");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidCredentials, result.Error!.Type);
    }

    [Fact]
    public async Task LogInAsync_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        // Act
        var result = await sut.LogInAsync("user", "pass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    [Fact]
    public async Task LogInAsync_Should_ReturnPasswordChangeRequired_When_ProblemTypeIsPasswordChangeRequired()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, Fakes.Problem("auth/password-change-required"), HttpStatusCode.Forbidden));

        // Act
        var result = await sut.LogInAsync("user", "pass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.PasswordChangeRequired, result.Error!.Type);
    }

    [Fact]
    public async Task LogInAsync_Should_ReturnUndefined_When_ProblemTypeIsUnknown()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, Fakes.Problem("server/internal-error"), HttpStatusCode.InternalServerError));

        // Act
        var result = await sut.LogInAsync("user", "pass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
    }

    [Fact]
    public async Task LogInAsync_Should_ReturnFallbackDetail_When_ProblemDetailsIsNull()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, null, HttpStatusCode.InternalServerError));

        // Act
        var result = await sut.LogInAsync("user", "pass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.Undefined, result.Error!.Type);
        Assert.Equal("An unexpected error occurred.", result.Error.Detail);
    }

    // --- Refresh ---

    [Fact]
    public async Task Refresh_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(true, Fakes.AuthResult(), null, HttpStatusCode.OK));

        // Act
        var result = await sut.Refresh("refresh-token");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Refresh_Should_ReturnUnauthorizedAccess_When_ProblemTypeIsUnauthorized()
    {
        // Arrange
        httpClient
            .PostFromJsonAsync<object, AuthResult>(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<AuthResult>(false, null, Fakes.Problem("auth/unauthorized"), HttpStatusCode.Unauthorized));

        // Act
        var result = await sut.Refresh("expired-token");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.UnauthorizedAccess, result.Error!.Type);
    }

    // --- ChangePassword ---

    [Fact]
    public async Task ChangePassword_Should_ReturnSuccess_When_HttpResponseIsSuccess()
    {
        // Arrange
        httpClient
            .PutJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(true, null, null, HttpStatusCode.NoContent));

        // Act
        var result = await sut.ChangePassword("new-pass", "old-pass");

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task ChangePassword_Should_ReturnInvalidCredentials_When_ProblemTypeIsInvalidPassword()
    {
        // Arrange
        httpClient
            .PutJsonAsync(Arg.Any<string>(), Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(new FargoSdkHttpResponse<EmptyResult>(false, null, Fakes.Problem("auth/invalid-password"), HttpStatusCode.BadRequest));

        // Act
        var result = await sut.ChangePassword("new-pass", "wrong-old-pass");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(FargoSdkErrorType.InvalidCredentials, result.Error!.Type);
    }

    private static class Fakes
    {
        public static AuthResult AuthResult() =>
            new("access-token", "refresh-token", DateTimeOffset.UtcNow.AddHours(1));

        public static FargoProblemDetails Problem(string type, string detail = "An error occurred.") =>
            new() { Type = type, Detail = detail };
    }
}
