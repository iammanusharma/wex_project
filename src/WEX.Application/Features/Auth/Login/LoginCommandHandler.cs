using MediatR;
using Microsoft.Extensions.Logging;
using WEX.Application.Interfaces;
using WEX.Domain.Exceptions;

namespace WEX.Application.Features.Auth.Login;

/// <summary>
/// Validates credentials and issues a JWT access token on success.
///
/// Security decisions:
/// - Never log the password
/// - Same exception for wrong username OR wrong password (prevents username enumeration)
/// - Delegate token creation to ITokenService (no JWT library in Application layer)
/// </summary>
public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserStore _userStore;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserStore userStore,
        ITokenService tokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userStore = userStore;
        _tokenService = tokenService;
        _logger = logger;
    }

    public Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Login attempt for user {Username}", request.Username);

        if (!_userStore.ValidateCredentials(request.Username, request.Password))
        {
            _logger.LogWarning("Failed login attempt for user {Username}", request.Username);
            throw new InvalidCredentialsException();
        }

        var (token, expiresIn) = _tokenService.CreateToken(request.Username);

        _logger.LogInformation("Successful login for user {Username}", request.Username);

        return Task.FromResult(new LoginResponse(token, expiresIn));
    }
}
