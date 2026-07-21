using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.AssignMechanic;
using APFMech.Application.WorkOrders.Commands.CompleteWorkOrder;
using APFMech.Domain.Entities;
using NSubstitute;

namespace APFMech.UnitTests.Application.WorkOrders.Commands;

public class WorkOrderCommandHandlerTests
{
    [Fact]
    public async Task AssignMechanicHandler_ShouldReturnNull_WhenWorkOrderDoesNotExist()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);
        workOrdersRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((WorkOrder?)null);

        var handler = new AssignMechanicCommandHandler(context);

        var result = await handler.Handle(new AssignMechanicCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignMechanicHandler_ShouldReturnNull_WhenMechanicIsInactive()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);

        var workOrder = WorkOrder.Create("Replace brake pads");
        var mechanic = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);
        mechanic.Deactivate();

        workOrdersRepository.GetByIdAsync(workOrder.Id, Arg.Any<CancellationToken>())
            .Returns(workOrder);
        employeesRepository.GetByIdAsync(mechanic.Id, Arg.Any<CancellationToken>())
            .Returns(mechanic);

        var handler = new AssignMechanicCommandHandler(context);

        var result = await handler.Handle(new AssignMechanicCommand(workOrder.Id, mechanic.Id), CancellationToken.None);

        Assert.Null(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignMechanicHandler_ShouldReturnNull_WhenMechanicDoesNotHaveMechanicRole()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);

        var workOrder = WorkOrder.Create("Replace brake pads");
        var mechanic = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Inspector"]);

        workOrdersRepository.GetByIdAsync(workOrder.Id, Arg.Any<CancellationToken>())
            .Returns(workOrder);
        employeesRepository.GetByIdAsync(mechanic.Id, Arg.Any<CancellationToken>())
            .Returns(mechanic);

        var handler = new AssignMechanicCommandHandler(context);

        var result = await handler.Handle(new AssignMechanicCommand(workOrder.Id, mechanic.Id), CancellationToken.None);

        Assert.Null(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AssignMechanicHandler_ShouldAssignMechanic_AndReturnDto()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);

        var workOrder = WorkOrder.Create("Replace brake pads");
        var mechanic = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);

        workOrdersRepository.GetByIdAsync(workOrder.Id, Arg.Any<CancellationToken>())
            .Returns(workOrder);
        employeesRepository.GetByIdAsync(mechanic.Id, Arg.Any<CancellationToken>())
            .Returns(mechanic);

        var handler = new AssignMechanicCommandHandler(context);

        var result = await handler.Handle(new AssignMechanicCommand(workOrder.Id, mechanic.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("InProgress", result!.Status);
        Assert.Equal(mechanic.Id, result.AssignedMechanicId);
        Assert.Equal("Alex Turner", result.AssignedMechanicFullName);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteWorkOrderHandler_ShouldReturnNull_WhenWorkOrderDoesNotExist()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);
        workOrdersRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((WorkOrder?)null);

        var handler = new CompleteWorkOrderCommandHandler(context);

        var result = await handler.Handle(new CompleteWorkOrderCommand(Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
        await context.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CompleteWorkOrderHandler_ShouldCompleteWorkOrder_AndReturnDto()
    {
        var context = Substitute.For<IApplicationDbContext>();
        var workOrdersRepository = Substitute.For<IWorkOrderRepository>();
        var employeesRepository = Substitute.For<IEmployeeRepository>();
        context.WorkOrders.Returns(workOrdersRepository);
        context.Employees.Returns(employeesRepository);

        var mechanic = Employee.Create(Guid.NewGuid(), "Alex", "Turner", ["Mechanic"]);
        var workOrder = WorkOrder.Create("Replace brake pads");
        workOrder.AssignMechanic(mechanic.Id);

        workOrdersRepository.GetByIdAsync(workOrder.Id, Arg.Any<CancellationToken>())
            .Returns(workOrder);
        employeesRepository.GetByIdAsync(mechanic.Id, Arg.Any<CancellationToken>())
            .Returns(mechanic);

        var handler = new CompleteWorkOrderCommandHandler(context);

        var result = await handler.Handle(new CompleteWorkOrderCommand(workOrder.Id), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Completed", result!.Status);
        Assert.Equal(mechanic.Id, result.AssignedMechanicId);
        Assert.Equal("Alex Turner", result.AssignedMechanicFullName);
        Assert.NotNull(workOrder.CompletedAtUtc);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
