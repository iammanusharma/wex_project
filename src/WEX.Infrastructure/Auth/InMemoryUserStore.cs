using Microsoft.Extensions.Configuration;
using WEX.Application.Interfaces;

namespace WEX.Infrastructure.Auth;

/// <summary>
/// Validates credentials against a list of test users stored in configuration.
///
/// This is intentionally simple for demo/interview purposes.
/// In production, replace with a database-backed user store (e.g. ASP.NET Core Identity).
///
/// Passwords are stored as plain text in config for demo only.
/// In production, store bcrypt hashes and use a constant-time comparison.
/// </summary>
public sealed class InMemoryUserStore : IUserStore
{
    private readonly Dictionary<string, string> _users;

    public InMemoryUserStore(IConfiguration configuration)
    {
        // Load users from config section "Auth:Users" as key=username, value=password
        _users = configuration
            .GetSection("Auth:Users")
            .GetChildren()
            .ToDictionary(
                s => s.Key,
                s => s.Value ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }

    public bool ValidateCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return false;

        return _users.TryGetValue(username, out var storedPassword)
               && storedPassword == password;
    }
}
