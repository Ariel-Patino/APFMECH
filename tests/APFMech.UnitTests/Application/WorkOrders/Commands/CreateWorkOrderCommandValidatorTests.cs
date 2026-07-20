using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using Xunit;

namespace APFMech.UnitTests.Application.WorkOrders.Commands;

public class CreateWorkOrderCommandValidatorTests
{
private readonly CreateWorkOrderCommandValidator _validator = new();
[Fact]
public void Validator_ShouldHaveError_WhenDescriptionIsEmpty()
{
    // Arrange
    var command = new CreateWorkOrderCommand(string.Empty);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.Description) 
                                        && e.ErrorMessage.Contains("required"));
}

[Fact]
public void Validator_ShouldHaveError_WhenDescriptionExceedsMaxLength()
{
    // Arrange
    var longDescription = new string('A', 501);
    var command = new CreateWorkOrderCommand(longDescription);

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateWorkOrderCommand.Description) 
                                        && e.ErrorMessage.Contains("exceed 500 characters"));
}

[Fact]
public void Validator_ShouldBeValid_WhenDescriptionIsWithinLimits()
{
    // Arrange
    var command = new CreateWorkOrderCommand("Standard maintenance on Pump-02.");

    // Act
    var result = _validator.Validate(command);

    // Assert
    Assert.True(result.IsValid);
    Assert.Empty(result.Errors);
}

}