using System.Text;
using Large_Scale_CommunityPlatform.Configurations;
using Large_Scale_CommunityPlatform.Services;
using Large_Scale_CommunityPlatform.Services.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Large_Scale_CommunityPlatform.Extensions;

public static class JwtAuthenticationExtension
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings")
            );

        var jwtSettings = configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>();

        if (jwtSettings == null || string.IsNullOrWhiteSpace(jwtSettings.Secret))
        {
            throw new InvalidOperationException("Jwt Settings: Secret is missing");
        }

        services.AddScoped<AuthService>();
        services.AddScoped<JwtTokenProvider>();
        services.AddScoped<JwtRefreshTokenProvider>();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                        
                    ValidateLifetime = true,
                    
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSettings.Secret)
                        ),
                    ClockSkew = TimeSpan.Zero
                };
            });
        
        return services;
    }
}