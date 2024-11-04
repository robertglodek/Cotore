using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Cotore.Auth.Jwt;

public static class Extensions
{
    private const string SectionName = "cotore:jwt";
    private const string RegistryName = "auth";

    public static ICotoreBuilder AddJwt(this ICotoreBuilder builder, string sectionName = SectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        var section = builder.Configuration.GetSection(sectionName);
        var options = section.BindOptions<JwtOptions>();
        builder.Services.Configure<JwtOptions>(section);

        if (!builder.TryRegister(RegistryName))
        {
            return builder;
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            RequireAudience = options.RequireAudience,
            ValidIssuer = options.ValidIssuer,
            ValidIssuers = options.ValidIssuers,
            ValidateActor = options.ValidateActor,
            ValidAudience = options.ValidAudience,
            ValidAudiences = options.ValidAudiences,
            ValidateAudience = options.ValidateAudience,
            ValidateIssuer = options.ValidateIssuer,
            ValidateLifetime = options.ValidateLifetime,
            ValidateTokenReplay = options.ValidateTokenReplay,
            ValidateIssuerSigningKey = options.ValidateIssuerSigningKey,
            SaveSigninToken = options.SaveSigninToken,
            RequireExpirationTime = options.RequireExpirationTime,
            RequireSignedTokens = options.RequireSignedTokens,
            ClockSkew = TimeSpan.Zero
        };

        if (!string.IsNullOrWhiteSpace(options.AuthenticationType))
        {
            tokenValidationParameters.AuthenticationType = options.AuthenticationType;
        }

        if (!string.IsNullOrWhiteSpace(options.IssuerSigningKey))
        {
            tokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(options.IssuerSigningKey));
        }

        if (!string.IsNullOrWhiteSpace(options.NameClaimType))
        {
            tokenValidationParameters.NameClaimType = options.NameClaimType;
        }

        if (!string.IsNullOrWhiteSpace(options.RoleClaimType))
        {
            tokenValidationParameters.RoleClaimType = options.RoleClaimType;
        }

        builder.Services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(jwtBearerOptions =>
        {
            jwtBearerOptions.Authority = options.Authority;
            jwtBearerOptions.Audience = options.Audience;
            jwtBearerOptions.MetadataAddress = options.MetadataAddress ?? string.Empty;
            jwtBearerOptions.SaveToken = options.SaveToken;
            jwtBearerOptions.RefreshOnIssuerKeyNotFound = options.RefreshOnIssuerKeyNotFound;
            jwtBearerOptions.RequireHttpsMetadata = options.RequireHttpsMetadata;
            jwtBearerOptions.IncludeErrorDetails = options.IncludeErrorDetails;
            jwtBearerOptions.TokenValidationParameters = tokenValidationParameters;
            if (!string.IsNullOrWhiteSpace(options.Challenge))
            {
                jwtBearerOptions.Challenge = options.Challenge;
            }
        });

        builder.Services.AddAuthorization();

        return builder;
    }
}
