using APFMech.Application.Common.Interfaces;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using APFMech.Domain.Entities;
using NSubstitute;
using Xunit;

namespace APFMech.UnitTests.Application.WorkOrders.Commands;

public class CreateWorkOrderCommandHandlerTests
{
private readonly IApplicationDbContext _context;
private readonly IWorkOrderRepository _repository;
private readonly CreateWorkOrderCommandHandler _handler;
public CreateWorkOrderCommandHandlerTests()
{
    _context = Substitute.For<IApplicationDbContext>();
    _repository = Substitute.For<IWorkOrderRepository>();
    
    // Setup the mock context to return the mock repository
    _context.WorkOrders.Returns(_repository);
    
    _handler = new CreateWorkOrderCommandHandler(_context);
}

[Fact]
public async Task Handle_ShouldPersistWorkOrderAndReturnDto()
{
    // Arrange
    var command = new CreateWorkOrderCommand("Fix hydraulic leak");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    // Verify the repository AddAsync was called with a WorkOrder matching the description
    await _repository.Received(1).AddAsync(
    Arg.Is<WorkOrder>(w => w != null && w.Description == command.Description),
    Arg.Any<CancellationToken>());
    
    // Verify SaveChangesAsync was called
    await _context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

    // Verify the returned DTO
    Assert.NotNull(result);
    Assert.Equal(command.Description, result.Description);
    Assert.Equal("Pending", result.Status);
}
}