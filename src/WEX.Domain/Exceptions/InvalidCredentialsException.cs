namespace WEX.Domain.Exceptions;

/// <summary>
/// Thrown when login credentials are invalid.
/// Maps to HTTP 401 Unauthorized.
///
/// Deliberately generic — does not reveal whether username or password was wrong
/// to prevent username enumeration attacks.
/// </summary>
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid username or password.")
    {
    }
}
