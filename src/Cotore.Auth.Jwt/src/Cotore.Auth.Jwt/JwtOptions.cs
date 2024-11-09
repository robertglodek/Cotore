namespace Cotore.Auth.Jwt;

internal sealed class JwtOptions
{
    public string? IssuerSigningKey { get; init; }
    public string? Authority { get; init; }
    public string? Audience { get; init; }
    public string Challenge { get; init; } = "Bearer";
    public string? MetadataAddress { get; init; }
    public bool SaveToken { get; init; } = true;
    public bool SaveSigninToken { get; init; }
    public bool RequireAudience { get; init; } = true;
    public bool RequireHttpsMetadata { get; init; } = true;
    public bool RequireExpirationTime { get; init; } = true;
    public bool RequireSignedTokens { get; init; } = true;
    public string? ValidAudience { get; init; }
    public IEnumerable<string>? ValidAudiences { get; init; }
    public string? ValidIssuer { get; init; }
    public IEnumerable<string>? ValidIssuers { get; init; }
    public bool ValidateActor { get; init; }
    public bool ValidateAudience { get; init; } = true;
    public bool ValidateIssuer { get; init; } = true;
    public bool ValidateLifetime { get; init; } = true;
    public bool ValidateTokenReplay { get; init; }
    public bool ValidateIssuerSigningKey { get; init; }
    public bool RefreshOnIssuerKeyNotFound { get; init; } = true;
    public bool IncludeErrorDetails { get; init; } = true;
    public string? AuthenticationType { get; init; }
    public string? NameClaimType { get; init; }
    public string? RoleClaimType { get; init; }
}