using MediatR;

namespace WEX.Application.Features.Auth.Login;

/// <summary>Authenticates a user and returns a JWT access token.</summary>
public sealed record LoginCommand(
    string Username,
    string Password) : IRequest<LoginResponse>;

/// <summary>Returned on successful authentication.</summary>
public sealed record LoginResponse(
    string AccessToken,
    int ExpiresIn);
