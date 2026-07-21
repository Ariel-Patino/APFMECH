using APFMech.Application.Common.Interfaces;
using APFMech.Infrastructure.Identity;
using APFMech.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace APFMech.Infrastructure.Persistence;

public class DatabaseInitializer(
    ApplicationDbContext dbContext,
    UserManager<User> userManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IOpenIddictApplicationManager applicationManager,
    IOpenIddictScopeManager scopeManager,
    IOptions<OpenIddictSeedOptions> seedOptions,
    IHostEnvironment environment,
    DatabaseInitializerForDevelopment devInitializer) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.EnsureCreatedAsync(cancellationToken);
        await SeedRolesAsync(cancellationToken);
        await SeedOpenIddictScopesAsync(cancellationToken);
        await SeedOpenIddictClientsAsync(cancellationToken);

        if (environment.IsDevelopment())
        {
            await SeedDevelopmentAdminAsync(cancellationToken);
            await devInitializer.SeedAsync(cancellationToken);
        }
    }

    private async Task SeedOpenIddictScopesAsync(CancellationToken cancellationToken)
    {
        if (await scopeManager.FindByNameAsync("apfmech_api", cancellationToken) is not null)
        {
            return;
        }

        await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
        {
            Name = "apfmech_api",
            DisplayName = "APFMech API",
            Resources = { "apfmech_resource_server" }
        }, cancellationToken);
    }

    private async Task SeedOpenIddictClientsAsync(CancellationToken cancellationToken)
    {
        var options = seedOptions.Value;
        if (await applicationManager.FindByClientIdAsync(options.AngularSpaClientId, cancellationToken) is not null)
        {
            return;
        }

        var descriptor = new OpenIddictApplicationDescriptor
        {
            ApplicationType = ApplicationTypes.Web,
            ClientId = options.AngularSpaClientId,
            ClientType = ClientTypes.Public,
            ConsentType = ConsentTypes.Explicit,
            DisplayName = "APFMech Angular SPA",
            Permissions =
            {
                Permissions.Endpoints.Authorization,
                Permissions.Endpoints.Token,
                Permissions.Endpoints.EndSession,
                Permissions.GrantTypes.AuthorizationCode,
                Permissions.GrantTypes.RefreshToken,
                Permissions.ResponseTypes.Code,
                Permissions.Scopes.Profile,
                Permissions.Scopes.Email,
                Permissions.Scopes.Roles,
                Permissions.Prefixes.Scope + Scopes.OpenId,
                Permissions.Prefixes.Scope + "apfmech_api"
            },
            Requirements =
            {
                Requirements.Features.ProofKeyForCodeExchange
            }
        };

        foreach (var redirectUri in options.AngularSpaRedirectUris)
        {
            descriptor.RedirectUris.Add(new Uri(redirectUri));
        }

        foreach (var logoutUri in options.AngularSpaPostLogoutRedirectUris)
        {
            descriptor.PostLogoutRedirectUris.Add(new Uri(logoutUri));
        }

        await applicationManager.CreateAsync(descriptor, cancellationToken);
    }

    private async Task SeedRolesAsync(CancellationToken cancellationToken)
    {
        var roles = new[] { "Mechanic", "Manager", "Admin" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid> { Name = role });
            }
        }
    }

    private async Task SeedDevelopmentAdminAsync(CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@apfmech.local";
        const string adminPassword = "Admin123!";

        if (await userManager.FindByEmailAsync(adminEmail) is not null)
        {
            return;
        }

        var user = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator"
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to seed development admin user: {string.Join(", ", result.Errors.Select(x => x.Description))}");
        }

        await userManager.AddToRolesAsync(user, ["Admin"]);

        var employee = Employee.Create(user.Id, user.FirstName, user.LastName, ["Admin"]);
        await dbContext.Employees.AddAsync(employee, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}