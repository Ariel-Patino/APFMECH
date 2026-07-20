using System.Security.Claims;
using APFMech.Application.Common.Interfaces;
using APFMech.Application.Employees.Commands.CreateEmployee;
using APFMech.Application.Employees.Queries.GetEmployeeByUserId;
using APFMech.Infrastructure.Identity;
using APFMech.WebAPI.Contracts.Auth;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace APFMech.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ISender sender) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return Unauthorized();
        }

        return NoContent();
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return NoContent();
    }

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> AuthorizeEndpoint(CancellationToken cancellationToken)
    {
        var scopes = Request.Query["scope"].ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (result is not { Succeeded: true })
        {
            return Challenge(
                authenticationSchemes: IdentityConstants.ApplicationScheme,
                properties: new AuthenticationProperties { RedirectUri = Request.PathBase + Request.Path + Request.QueryString });
        }

        var user = await userManager.GetUserAsync(result.Principal);
        if (user is null)
        {
            return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        var principal = await CreatePrincipalAsync(user, scopes);
        return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Exchange(CancellationToken cancellationToken)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest(new { error = "invalid_request" });
        }

        var form = await Request.ReadFormAsync(cancellationToken);
        var grantType = form["grant_type"].ToString();

        if (string.Equals(grantType, GrantTypes.AuthorizationCode, StringComparison.Ordinal) ||
            string.Equals(grantType, GrantTypes.RefreshToken, StringComparison.Ordinal))
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            if (authenticateResult.Principal is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var user = await userManager.GetUserAsync(authenticateResult.Principal);
            if (user is null)
            {
                return Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            }

            var principal = await CreatePrincipalAsync(user, authenticateResult.Principal.GetScopes());
            return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return BadRequest(new { error = "unsupported_grant_type" });
    }

    [HttpGet("~/connect/userinfo")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UserInfo()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);

        return Ok(new
        {
            sub = user.Id,
            email = user.Email,
            name = $"{user.FirstName} {user.LastName}".Trim(),
            given_name = user.FirstName,
            family_name = user.LastName,
            role = roles
        });
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthMeResponse>> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var user = new User
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var createUserResult = await userManager.CreateAsync(user, request.Password);
        if (!createUserResult.Succeeded)
        {
            return BadRequest(createUserResult.Errors.Select(error => error.Description));
        }

        var defaultRoles = new[] { "Mechanic" };
        var addRolesResult = await userManager.AddToRolesAsync(user, defaultRoles);
        if (!addRolesResult.Succeeded)
        {
            return BadRequest(addRolesResult.Errors.Select(error => error.Description));
        }

        await sender.Send(new CreateEmployeeCommand(user.Id, request.FirstName, request.LastName, defaultRoles), cancellationToken);

        return Ok(await BuildCurrentUserResponseAsync(user));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthMeResponse>> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue(Claims.Subject);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        return Ok(await BuildCurrentUserResponseAsync(user));
    }

    private async Task<AuthMeResponse> BuildCurrentUserResponseAsync(User user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var employee = await sender.Send(new GetEmployeeByUserIdQuery(user.Id));

        return new AuthMeResponse(
            user.Id,
            user.Email ?? user.UserName ?? string.Empty,
            $"{user.FirstName} {user.LastName}".Trim(),
            roles.ToArray(),
            employee?.Id,
            employee is not null);
    }

    private async Task<ClaimsPrincipal> CreatePrincipalAsync(User user, IEnumerable<string> scopes)
    {
        var roles = await userManager.GetRolesAsync(user);
        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(Claims.Subject, user.Id.ToString())
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        identity.AddClaim(new Claim(Claims.Email, user.Email ?? user.UserName ?? string.Empty)
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        identity.AddClaim(new Claim(Claims.Name, $"{user.FirstName} {user.LastName}".Trim())
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        identity.AddClaim(new Claim(Claims.GivenName, user.FirstName)
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        identity.AddClaim(new Claim(Claims.FamilyName, user.LastName)
            .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));

        foreach (var role in roles)
        {
            identity.AddClaim(new Claim(Claims.Role, role)
                .SetDestinations(Destinations.AccessToken, Destinations.IdentityToken));
        }

        var principal = new ClaimsPrincipal(identity);
        principal.SetScopes(scopes);
        principal.SetResources("apfmech_resource_server");

        return principal;
    }
}