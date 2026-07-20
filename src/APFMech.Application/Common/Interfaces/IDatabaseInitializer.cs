namespace APFMech.Application.Common.Interfaces;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}