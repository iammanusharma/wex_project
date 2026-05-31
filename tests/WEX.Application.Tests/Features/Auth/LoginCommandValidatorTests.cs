using FluentAssertions;
using FluentValidation.TestHelper;
using WEX.Application.Features.Auth.Login;
using Xunit;

namespace WEX.Application.Tests.Features.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new LoginCommand("demo@wex.com", "WexDemo2024!");

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "Password123")]
    [InlineData(null, "Password123")]
    public void Validate_MissingUsername_HasValidationError(string? username, string password)
    {
        // Arrange
        var command = new LoginCommand(username!, password);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Username);
    }

    [Theory]
    [InlineData("user@wex.com", "")]
    [InlineData("user@wex.com", null)]
    public void Validate_MissingPassword_HasValidationError(string username, string? password)
    {
        // Arrange
        var command = new LoginCommand(username, password!);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Password);
    }
}
