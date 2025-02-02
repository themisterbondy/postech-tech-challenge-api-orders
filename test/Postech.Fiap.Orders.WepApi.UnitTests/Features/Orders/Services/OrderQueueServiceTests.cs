using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Repositories;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Services;

public class OrderQueueServiceTests
{
    private readonly IOrderQueueRepository _orderQueueRepository;
    private readonly OrderQueueService _orderQueueService;

    public OrderQueueServiceTests()
    {
        _orderQueueRepository = Substitute.For<IOrderQueueRepository>();
        _orderQueueService = new OrderQueueService(_orderQueueRepository);
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderQueueRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(null));

        // Act
        var result = await _orderQueueService.GetOrderByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain($"Order with id {orderId} not found.");
    }

    [Fact]
    public async Task GetOrderByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderId = OrderId.New();
        var orderQueue = CreateMockOrderQueue(orderId);
        _orderQueueRepository.GetByIdAsync(orderId.Value, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(orderQueue));

        // Act
        var result = await _orderQueueService.GetOrderByIdAsync(orderId.Value, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.OrderId.Should().Be(orderId.Value);
        result.Value.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderQueueRepository.GetByIdAsync(orderId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(null));

        // Act
        var result =
            await _orderQueueService.UpdateOrderStatusAsync(orderId, OrderQueueStatus.Completed,
                CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain($"Order with id {orderId} not found.");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_ShouldUpdateStatus_WhenOrderExists()
    {
        // Arrange
        var orderId = OrderId.New();
        var orderQueue = CreateMockOrderQueue(orderId);
        _orderQueueRepository.GetByIdAsync(orderId.Value, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(orderQueue));

        // Act
        var result =
            await _orderQueueService.UpdateOrderStatusAsync(orderId.Value, OrderQueueStatus.Completed,
                CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be(OrderQueueStatus.Completed);
        await _orderQueueRepository.Received(1)
            .UpdateStatusAsync(orderId.Value, OrderQueueStatus.Completed, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetOrderByTransactionIdAsync_ShouldReturnFailure_WhenOrderNotFound()
    {
        // Arrange
        var transactionId = "TX123";
        _orderQueueRepository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(null));

        // Act
        var result = await _orderQueueService.GetOrderByTransactionIdAsync(transactionId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain($"Order with transactionId {transactionId} not found.");
    }

    [Fact]
    public async Task GetOrderByTransactionIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var transactionId = "TX123";
        var orderQueue = CreateMockOrderQueue(OrderId.New(), transactionId);
        _orderQueueRepository.GetByTransactionIdAsync(transactionId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<OrderQueue?>(orderQueue));

        // Act
        var result = await _orderQueueService.GetOrderByTransactionIdAsync(transactionId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TransactionId.Should().Be(transactionId);
    }

    private static OrderQueue CreateMockOrderQueue(OrderId orderId, string transactionId = "TX123")
    {
        var items = new List<OrderItem>
        {
            OrderItem.Create(
                OrderItemId.New(),
                orderId,
                new ProductId(Guid.NewGuid()),
                "Product 1",
                10.99m,
                2,
                ProductCategory.Lanche
            ),
            OrderItem.Create(
                OrderItemId.New(),
                orderId,
                new ProductId(Guid.NewGuid()),
                "Product 2",
                20.49m,
                1,
                ProductCategory.Acompanhamento
            )
        };

        return OrderQueue.Create(
            orderId,
            Guid.NewGuid(),
            items,
            transactionId,
            OrderQueueStatus.Received
        );
    }
}