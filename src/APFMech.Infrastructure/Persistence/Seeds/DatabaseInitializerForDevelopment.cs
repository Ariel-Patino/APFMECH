using APFMech.Domain.Entities;
using APFMech.Domain.Entities.Enums;
using APFMech.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace APFMech.Infrastructure.Persistence;

public class DatabaseInitializerForDevelopment(
    ApplicationDbContext dbContext,
    UserManager<User> userManager,
    IHostEnvironment environment)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        await SeedUsersAndEmployeesAsync(cancellationToken);
        await SeedWorkOrdersAsync(cancellationToken);
    }

    private async Task SeedUsersAndEmployeesAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.Users.CountAsync(cancellationToken) > 1)
        {
            return;
        }

        const string defaultPassword = "Password123!";

        var newUsersData = new[]
        {
            new { Email = "manager1@apfmech.local", FirstName = "Carlos", LastName = "Mendoza", Role = UserRole.Manager },
            new { Email = "manager2@apfmech.local", FirstName = "Valeria", LastName = "Rios", Role = UserRole.Manager },
            new { Email = "mechanic1@apfmech.local", FirstName = "Jorge", LastName = "Silva", Role = UserRole.Mechanic },
            new { Email = "mechanic2@apfmech.local", FirstName = "Mateo", LastName = "Torres", Role = UserRole.Mechanic }
        };

        foreach (var userData in newUsersData)
        {
            if (await userManager.FindByEmailAsync(userData.Email) is not null)
            {
                continue;
            }

            var user = new User
            {
                UserName = userData.Email,
                Email = userData.Email,
                EmailConfirmed = true,
                FirstName = userData.FirstName,
                LastName = userData.LastName
            };

            var result = await userManager.CreateAsync(user, defaultPassword);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to seed user {userData.Email}: {string.Join(", ", result.Errors.Select(x => x.Description))}");
            }

            var roleName = userData.Role.ToString();
            await userManager.AddToRoleAsync(user, roleName);

            var employee = Employee.Create(user.Id, user.FirstName, user.LastName, [roleName]);
            await dbContext.Employees.AddAsync(employee, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedWorkOrdersAsync(CancellationToken cancellationToken)
    {
        if (await dbContext.WorkOrders.AnyAsync(cancellationToken))
        {
            return;
        }

        var realWorkOrderDescriptions = new[]
        {
            "Replace front brake pads and resurface rotors on Fleet Truck #102.",
            "Diagnose engine overheating and check coolant pressure in CAT Excavator.",
            "Perform 10,000 km routine maintenance (oil, air filter, spark plugs) on Service Van B.",
            "Inspect hydraulic hose leak in Main Crane Lift System.",
            "Replace worn alternator and test battery charge output on Forklift #4.",
            "Transmission fluid flush and gear shifting diagnostic on Haul Truck A.",
            "Repair broken tail light assembly and wire harness on Delivery Trailer #08.",
            "Calibrate wheel alignment and replace worn front tires on Service Van A.",
            "Perform AC system recharge and replace cabin filter in Fleet Pickup #201.",
            "Inspect and adjust clutch cable tension on Flatbed Transport #12."
        };

        foreach (var description in realWorkOrderDescriptions)
        {
            var workOrder = WorkOrder.Create(description);
            await dbContext.WorkOrders.AddAsync(workOrder, cancellationToken);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}