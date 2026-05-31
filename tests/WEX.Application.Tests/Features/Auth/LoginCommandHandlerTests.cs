using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using WEX.Application.Features.Auth.Login;
using WEX.Application.Interfaces;
using WEX.Domain.Exceptions;
using Xunit;

namespace WEX.Application.Tests.Features.Auth;

public class LoginCommandHandlerTests
{
    private readonly IUserStore _userStore;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _userStore = Substitute.For<IUserStore>();
        _tokenService = Substitute.For<ITokenService>();
        _logger = Substitute.For<ILogger<LoginCommandHandler>>();
        _handler = new LoginCommandHandler(_userStore, _tokenService, _logger);
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokenResponse()
    {
        // Arrange
        var command = new LoginCommand("demo@wex.com", "WexDemo2024!");
        _userStore.ValidateCredentials("demo@wex.com", "WexDemo2024!").Returns(true);
        _tokenService.CreateToken("demo@wex.com").Returns(("jwt_token_here", 3600));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt_token_here");
        result.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Handle_InvalidCredentials_ThrowsInvalidCredentialsException()
    {
        // Arrange
        var command = new LoginCommand("demo@wex.com", "WrongPassword");
        _userStore.ValidateCredentials(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidCredentialsException>();
        _tokenService.DidNotReceive().CreateToken(Arg.Any<string>());
    }

    [Fact]
    public async Task Handle_InvalidCredentials_DoesNotRevealWhichFieldIsWrong()
    {
        // Arrange — wrong username
        var command = new LoginCommand("nobody@wex.com", "anypassword");
        _userStore.ValidateCredentials(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        // Act & Assert — same exception type regardless of whether user or password is wrong
        await Assert.ThrowsAsync<InvalidCredentialsException>(
            () => _handler.Handle(command, CancellationToken.None));
    }
}
