using FluentValidation.TestHelper;
using Postech.Fiap.Orders.WebApi.Features.Orders.Commands;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Commands;

public class UpdateOrderQueueStatusCommandValidatorTests
{
    private readonly UpdateOrderQueueStatusCommand.Validator _validator;

    public UpdateOrderQueueStatusCommandValidatorTests()
    {
        _validator = new UpdateOrderQueueStatusCommand.Validator();
    }

    [Fact]
    public void Should_HaveError_When_IdIsEmpty()
    {
        // Arrange
        var command = new UpdateOrderQueueStatusCommand.Command { Id = Guid.Empty, Status = OrderQueueStatus.Received };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Id is required.");
    }

    [Fact]
    public void Should_HaveError_When_StatusIsInvalid()
    {
        // Arrange
        var command = new UpdateOrderQueueStatusCommand.Command { Id = Guid.NewGuid(), Status = (OrderQueueStatus)99 };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status is invalid.");
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new UpdateOrderQueueStatusCommand.Command
            { Id = Guid.NewGuid(), Status = OrderQueueStatus.Ready };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }
}