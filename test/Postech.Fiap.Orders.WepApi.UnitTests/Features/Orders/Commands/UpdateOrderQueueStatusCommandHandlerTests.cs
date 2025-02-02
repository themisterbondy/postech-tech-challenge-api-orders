using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Commands;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Commands;

public class UpdateOrderQueueStatusCommandHandlerTests
{
    private readonly UpdateOrderQueueStatusCommand.Handler _handler;
    private readonly IOrderQueueService _orderQueueService;

    public UpdateOrderQueueStatusCommandHandlerTests()
    {
        _orderQueueService = Substitute.For<IOrderQueueService>();
        _handler = new UpdateOrderQueueStatusCommand.Handler(_orderQueueService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderQueueService.UpdateOrderStatusAsync(orderId, OrderQueueStatus.Completed, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<EnqueueOrderResponse>(Error.Failure("", "Order not found")));

        var command = new UpdateOrderQueueStatusCommand.Command { Id = orderId, Status = OrderQueueStatus.Completed };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOrderStatusIsUpdated()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var response = new EnqueueOrderResponse
        {
            OrderId = orderId,
            Status = OrderQueueStatus.Completed
        };

        _orderQueueService.UpdateOrderStatusAsync(orderId, OrderQueueStatus.Completed, Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var command = new UpdateOrderQueueStatusCommand.Command { Id = orderId, Status = OrderQueueStatus.Completed };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);
    }
}