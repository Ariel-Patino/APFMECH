namespace APFMech.Infrastructure.Persistence;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    public string Provider { get; init; } = DatabaseProvider.Sqlite.ToString();
    public string ConnectionString { get; init; } = "Data Source=apfmech.db";
}

public enum DatabaseProvider
{
    Sqlite,
    SqlServer
}