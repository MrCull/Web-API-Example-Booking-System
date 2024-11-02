// In a new file called ServiceCollectionExtensions.cs
using Infrastructure.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Azure.Cosmos;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Api;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCosmosRepository(this IServiceCollection services, string databaseName, string containerName)
    {
        services.AddScoped<IRepository>(serviceProvider =>
        {
            CosmosClient cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();
            return new Repository(cosmosClient, databaseName, containerName);
        });
        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        JwtSettings? jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>();

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
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
            };
        });

        return services;
    }
}
