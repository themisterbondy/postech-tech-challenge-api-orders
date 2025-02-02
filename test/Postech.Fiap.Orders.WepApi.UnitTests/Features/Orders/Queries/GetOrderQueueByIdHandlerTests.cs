using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Queries;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Queries;

public class GetOrderQueueByIdHandlerTests
{
    private readonly GetOrderQueueById.Handler _handler;
    private readonly IOrderQueueService _orderQueueService;

    public GetOrderQueueByIdHandlerTests()
    {
        _orderQueueService = Substitute.For<IOrderQueueService>();
        _handler = new GetOrderQueueById.Handler(_orderQueueService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderQueueService.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<EnqueueOrderResponse>(Error.Failure("d", "Order not found")));

        var query = new GetOrderQueueById.Query { Id = orderId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Be("Order not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var response = new EnqueueOrderResponse
        {
            OrderId = orderId,
            CreatedAt = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            Status = OrderQueueStatus.Received,
            TransactionId = "TX123",
            Items = []
        };

        _orderQueueService.GetOrderByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var query = new GetOrderQueueById.Query { Id = orderId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);
    }
}