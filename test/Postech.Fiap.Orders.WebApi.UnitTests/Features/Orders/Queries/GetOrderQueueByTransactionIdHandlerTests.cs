using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Queries;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Orders.Queries;

public class GetOrderQueueByTransactionIdHandlerTests
{
    private readonly GetOrderQueueByTransactionId.Handler _handler;
    private readonly IOrderQueueService _orderQueueService;

    public GetOrderQueueByTransactionIdHandlerTests()
    {
        _orderQueueService = Substitute.For<IOrderQueueService>();
        _handler = new GetOrderQueueByTransactionId.Handler(_orderQueueService);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var transactionId = "TX123";
        _orderQueueService.GetOrderByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<EnqueueOrderResponse>(Error.Failure("d", "Order not found")));

        var query = new GetOrderQueueByTransactionId.Query { TransactionId = transactionId };

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
        var transactionId = "TX123";
        var response = new EnqueueOrderResponse
        {
            OrderId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            CustomerId = Guid.NewGuid(),
            Status = OrderQueueStatus.Received,
            TransactionId = transactionId,
            Items = []
        };

        _orderQueueService.GetOrderByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns(Result.Success(response));

        var query = new GetOrderQueueByTransactionId.Query { TransactionId = transactionId };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(response);
    }
}