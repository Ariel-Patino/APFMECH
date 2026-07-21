using APFMech.Application.Common.Interfaces;
using APFMech.Application.Employees.Commands.DeleteEmployeeGdpr;
using APFMech.Application.Employees.Commands.DisableEmployee;
using APFMech.Domain.Entities;
using NSubstitute;

namespace APFMech.UnitTests.Application.Employees.Commands;

public class EmployeeCommandHandlerTests
{
    [Fact]
    public async Task DisableEmployeeHandler_ShouldReturnNull_WhenEmployeeDoesNotExist()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var repository = Substitute.For<IEmployeeRepository>();
        context.Employees.Returns(repository);
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var handler = new DisableEmployeeCommandHandler(context);

        var result = await handler.Handle(new DisableEmployeeCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        repository.DidNotReceive().Update(Arg.Any<Employee>());
    }

    [Fact]
    public async Task DisableEmployeeHandler_ShouldDeactivateEmployee_AndPersistChanges()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var repository = Substitute.For<IEmployeeRepository>();
        context.Employees.Returns(repository);

        var employee = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);
        repository.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>())
            .Returns(employee);

        var handler = new DisableEmployeeCommandHandler(context);

        var result = await handler.Handle(new DisableEmployeeCommand(employee.Id), CancellationToken.None);
        var disabledEmployee = result ?? throw new Xunit.Sdk.XunitException("Expected a disabled employee to be returned.");

        Assert.False(disabledEmployee.IsActive);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        var employeeId = employee.Id;
        repository.Received(1).Update(Arg.Is<Employee>(item => item != null && item.Id == employeeId && !item.IsActive));
    }

    [Fact]
    public async Task DeleteEmployeeHandler_ShouldReturnFalse_WhenEmployeeDoesNotExist()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var repository = Substitute.For<IEmployeeRepository>();
        var identityService = Substitute.For<IIdentityService>();
        context.Employees.Returns(repository);
        repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var handler = new DeleteEmployeeGdprCommandHandler(context, identityService);

        var result = await handler.Handle(new DeleteEmployeeGdprCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.False(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        await identityService.DidNotReceiveWithAnyArgs().DeleteUserAsync(default);
    }

    [Fact]
    public async Task DeleteEmployeeHandler_ShouldDeleteUserAndEmployee_WhenIdentityDeletionSucceeds()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var repository = Substitute.For<IEmployeeRepository>();
        var identityService = Substitute.For<IIdentityService>();
        context.Employees.Returns(repository);

        var employee = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);
        repository.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>())
            .Returns(employee);
        identityService.DeleteUserAsync(employee.UserId).Returns(true);

        var handler = new DeleteEmployeeGdprCommandHandler(context, identityService);

        var result = await handler.Handle(new DeleteEmployeeGdprCommand(employee.Id), CancellationToken.None);
        var employeeId = employee.Id;
        var userId = employee.UserId;

        Assert.True(result);
        await identityService.Received(1).DeleteUserAsync(userId);
        repository.Received(1).Remove(Arg.Is<Employee>(item => item != null && item.Id == employeeId));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteEmployeeHandler_ShouldReturnFalse_WhenIdentityDeletionFails()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var repository = Substitute.For<IEmployeeRepository>();
        var identityService = Substitute.For<IIdentityService>();
        context.Employees.Returns(repository);

        var employee = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);
        repository.GetByIdAsync(employee.Id, Arg.Any<CancellationToken>())
            .Returns(employee);
        identityService.DeleteUserAsync(employee.UserId).Returns(false);

        var handler = new DeleteEmployeeGdprCommandHandler(context, identityService);

        var result = await handler.Handle(new DeleteEmployeeGdprCommand(employee.Id), CancellationToken.None);

        Assert.False(result);
        repository.DidNotReceive().Remove(Arg.Any<Employee>());
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
