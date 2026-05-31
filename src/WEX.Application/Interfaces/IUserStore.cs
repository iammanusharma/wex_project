namespace WEX.Application.Interfaces;

/// <summary>
/// Validates user credentials.
/// Abstracted so the Application layer has no dependency on
/// the storage mechanism (config, database, LDAP, etc.).
/// </summary>
public interface IUserStore
{
    /// <summary>
    /// Returns true if the username and password are valid.
    /// Always returns false (never throws) for invalid credentials —
    /// the handler decides the response shape.
    /// </summary>
    bool ValidateCredentials(string username, string password);
}
