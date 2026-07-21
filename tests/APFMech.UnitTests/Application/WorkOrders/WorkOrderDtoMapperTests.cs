using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using APFMech.Domain.Entities;
using NSubstitute;

namespace APFMech.UnitTests.Application.WorkOrders;

public class WorkOrderDtoMapperTests
{
    [Fact]
    public async Task ToDtoAsync_ShouldResolveAssignedMechanicFullName_WhenMechanicExists()
    {
        var workOrder = WorkOrder.Create("Replace brake pads");
        var mechanicId = Guid.NewGuid();
        workOrder.AssignMechanic(mechanicId);

        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.GetByIdAsync(mechanicId, Arg.Any<CancellationToken>())
            .Returns(Employee.Create(mechanicId, "  Alex ", "  Turner "));

        var dto = await workOrder.ToDtoAsync(employeeRepository, CancellationToken.None);

        Assert.Equal(workOrder.Id, dto.Id);
        Assert.Equal(workOrder.TrackingNumber, dto.TrackingNumber);
        Assert.Equal(workOrder.Description, dto.Description);
        Assert.Equal("InProgress", dto.Status);
        Assert.Equal(mechanicId, dto.AssignedMechanicId);
        Assert.Equal("Alex Turner", dto.AssignedMechanicFullName);
    }

    [Fact]
    public async Task ToDtoAsync_ShouldReturnNullAssignedMechanicFullName_WhenMechanicDoesNotExist()
    {
        var workOrder = WorkOrder.Create("Replace brake pads");
        var mechanicId = Guid.NewGuid();
        workOrder.AssignMechanic(mechanicId);

        var employeeRepository = Substitute.For<IEmployeeRepository>();
        employeeRepository.GetByIdAsync(mechanicId, Arg.Any<CancellationToken>())
            .Returns((Employee?)null);

        var dto = await workOrder.ToDtoAsync(employeeRepository, CancellationToken.None);

        Assert.Equal(mechanicId, dto.AssignedMechanicId);
        Assert.Null(dto.AssignedMechanicFullName);
    }

    [Fact]
    public async Task ToDtoAsync_ShouldReturnNullAssignedMechanicFullName_WhenWorkOrderIsUnassigned()
    {
        var workOrder = WorkOrder.Create("Replace brake pads");
        var employeeRepository = Substitute.For<IEmployeeRepository>();

        var dto = await workOrder.ToDtoAsync(employeeRepository, CancellationToken.None);

        Assert.Null(dto.AssignedMechanicId);
        Assert.Null(dto.AssignedMechanicFullName);
        await employeeRepository.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
    }
}
