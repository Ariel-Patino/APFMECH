using APFMech.Application.Common.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace APFMech.Infrastructure.Identity;

public class IdentityService(UserManager<User> userManager) : IIdentityService
{
    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is not null;
    }

    public async Task<bool> IsInRoleAsync(Guid userId, string role)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;
        
        return await userManager.IsInRoleAsync(user, role);
    }

    public async Task<string?> GetUserNameAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user?.UserName;
    }
}