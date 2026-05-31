namespace WEX.Infrastructure.Auth;

/// <summary>
/// Typed configuration for JWT token generation.
/// Bound from appsettings "Jwt" section.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    /// <summary>Secret key used to sign tokens. Must be at least 32 chars in production.</summary>
    public string Secret { get; init; } = string.Empty;

    /// <summary>Token issuer (identifies this API).</summary>
    public string Issuer { get; init; } = "wex-api";

    /// <summary>Intended audience (identifies the client).</summary>
    public string Audience { get; init; } = "wex-client";

    /// <summary>Token validity in minutes. Default 60.</summary>
    public int ExpiryMinutes { get; init; } = 60;
}
