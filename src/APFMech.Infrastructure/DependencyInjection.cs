using APFMech.Application.Common.Interfaces;
using APFMech.Infrastructure.Identity;
using APFMech.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace APFMech.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistenceOptions = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>()
            ?? new PersistenceOptions();

        services.AddDbContext<ApplicationDbContext>(options => ConfigureDbContext(options, persistenceOptions));

        services.AddScoped<IApplicationDbContext>(provider => 
            provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<OpenIddictSeedOptions>(configuration.GetSection(OpenIddictSeedOptions.SectionName));

        // Set up Identity Services with custom GUID keys and cookie auth for OIDC authorization endpoint.
        services.AddIdentity<User, IdentityRole<Guid>>()
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "APFMech.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.LoginPath = "/api/auth/login";
            options.LogoutPath = "/api/auth/logout";
            options.SlidingExpiration = true;
        });

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IDatabaseInitializer, DatabaseInitializer>();

        services.AddOpenIddict()
            .AddCore(options => options.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
            .AddServer(options =>
            {
                options.SetAuthorizationEndpointUris("connect/authorize");
                options.SetTokenEndpointUris("connect/token");
                options.SetUserInfoEndpointUris("connect/userinfo");
                options.SetEndSessionEndpointUris("connect/logout");

                options.AllowAuthorizationCodeFlow()
                    .RequireProofKeyForCodeExchange();
                options.AllowRefreshTokenFlow();

                options.RegisterScopes(Scopes.OpenId, Scopes.Profile, Scopes.Email, Scopes.Roles, "apfmech_api");

                options.AddDevelopmentEncryptionCertificate();
                options.AddDevelopmentSigningCertificate();

                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(15));
                options.SetRefreshTokenLifetime(TimeSpan.FromDays(7));

                options.UseAspNetCore()
                    .EnableAuthorizationEndpointPassthrough()
                    .EnableTokenEndpointPassthrough()
                    .EnableUserInfoEndpointPassthrough()
                    .EnableEndSessionEndpointPassthrough();
            })
            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        return services;
    }

    private static void ConfigureDbContext(DbContextOptionsBuilder optionsBuilder, PersistenceOptions persistenceOptions)
    {
        var provider = Enum.TryParse<DatabaseProvider>(persistenceOptions.Provider, ignoreCase: true, out var parsedProvider)
            ? parsedProvider
            : DatabaseProvider.Sqlite;

        switch (provider)
        {
            case DatabaseProvider.SqlServer:
                optionsBuilder.UseSqlServer(persistenceOptions.ConnectionString);
                break;
            case DatabaseProvider.Sqlite:
            default:
                optionsBuilder.UseSqlite(persistenceOptions.ConnectionString);
                break;
        }
    }
}