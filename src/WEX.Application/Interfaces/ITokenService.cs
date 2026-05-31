namespace WEX.Application.Interfaces;

/// <summary>
/// Generates signed JWT access tokens.
/// Defined in Application so handlers depend on the abstraction, not the JWT library.
/// </summary>
public interface ITokenService
{
    /// <summary>Creates a signed JWT access token for the given username.</summary>
    (string Token, int ExpiresIn) CreateToken(string username);
}
