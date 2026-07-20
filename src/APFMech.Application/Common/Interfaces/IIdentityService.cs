namespace APFMech.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<bool> UserExistsAsync(Guid userId);
    Task<bool> IsInRoleAsync(Guid userId, string role);
    Task<string?> GetUserNameAsync(Guid userId);
}