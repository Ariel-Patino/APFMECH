namespace APFMech.Infrastructure.Persistence;

public sealed class OpenIddictSeedOptions
{
    public const string SectionName = "OpenIddictSeed";

    public string AngularSpaClientId { get; init; } = "apfmech-angular-spa";
    public string[] AngularSpaRedirectUris { get; init; } = ["http://localhost:4200/auth/callback"];
    public string[] AngularSpaPostLogoutRedirectUris { get; init; } = ["http://localhost:4200/"];
}