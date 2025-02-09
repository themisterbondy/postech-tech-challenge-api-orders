using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Queries;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;
using Postech.Fiap.Orders.WebApi.Persistence;
using Postech.Fiap.Orders.WebApi.UnitTests.Mocks;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Orders.Queries;

public class ListOrdersHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly ListOrders.Handler _handler;

    public ListOrdersHandlerTests()
    {
        _context = ApplicationDbContextMock.Create();
        _handler = new ListOrders.Handler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveOrders()
    {
        // Act
        var result = await _handler.Handle(new ListOrders.Query(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyActiveOrders()
    {
        // Arrange
        var activeOrder = CreateMockOrder(OrderQueueStatus.Received);
        var completedOrder = CreateMockOrder(OrderQueueStatus.Completed);
        var cancelledOrder = CreateMockOrder(OrderQueueStatus.Cancelled);

        await _context.OrderQueue.AddRangeAsync(activeOrder, completedOrder, cancelledOrder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new ListOrders.Query(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Orders.Should().HaveCount(1);
        result.Value.Orders[0].OrderId.Should().Be(activeOrder.Id.Value);
    }

    [Fact]
    public async Task Handle_ShouldReturnOrdersInCorrectPriorityOrder()
    {
        // Arrange
        var readyOrder = CreateMockOrder(OrderQueueStatus.Ready, DateTime.UtcNow.AddMinutes(-10));
        var preparingOrder = CreateMockOrder(OrderQueueStatus.Preparing, DateTime.UtcNow.AddMinutes(-20));
        var receivedOrder = CreateMockOrder(OrderQueueStatus.Received, DateTime.UtcNow.AddMinutes(-30));

        await _context.OrderQueue.AddRangeAsync(readyOrder, preparingOrder, receivedOrder);
        await _context.SaveChangesAsync();

        // Act
        var result = await _handler.Handle(new ListOrders.Query(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Orders.Should().HaveCount(3);
        result.Value.Orders[0].OrderId.Should().Be(readyOrder.Id.Value); // Ready primeiro
        result.Value.Orders[1].OrderId.Should().Be(preparingOrder.Id.Value); // Preparing segundo
        result.Value.Orders[2].OrderId.Should().Be(receivedOrder.Id.Value); // Received por último
    }

    private static OrderQueue CreateMockOrder(OrderQueueStatus status, DateTime? createdAt = null)
    {
        var orderId = OrderId.New();
        var items = new List<OrderItem>
        {
            OrderItem.Create(OrderItemId.New(), orderId, new ProductId(Guid.NewGuid()), "Product 1", 10.99m, 2,
                ProductCategory.Acompanhamento)
        };

        var order = OrderQueue.Create(orderId, Guid.NewGuid(), items, "TX123", status);

        if (createdAt.HasValue) order.CreatedAt = createdAt.Value;

        return order;
    }
}