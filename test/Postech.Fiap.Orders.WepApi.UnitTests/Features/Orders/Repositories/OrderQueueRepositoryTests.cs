using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Orders.Repositories;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;
using Postech.Fiap.Orders.WebApi.Persistence;
using Postech.Fiap.Orders.WepApi.UnitTests.Mocks;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Repositories;

public class OrderQueueRepositoryTests
{
    private readonly ApplicationDbContext _context;
    private readonly OrderQueueRepository _repository;

    public OrderQueueRepositoryTests()
    {
        _context = ApplicationDbContextMock.Create();
        _repository = new OrderQueueRepository(_context);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(orderId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderQueue = CreateMockOrderQueue();
        await _repository.AddAsync(orderQueue, CancellationToken.None);

        // Act
        var result = await _repository.GetByIdAsync(orderQueue.Id.Value, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(orderQueue.Id);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task AddAsync_ShouldPersistOrder()
    {
        // Arrange
        var orderQueue = CreateMockOrderQueue();

        // Act
        await _repository.AddAsync(orderQueue, CancellationToken.None);
        var result = await _context.OrderQueue.FirstOrDefaultAsync(o => o.Id == orderQueue.Id);

        // Assert
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be(orderQueue.TransactionId);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldChangeOrderStatus()
    {
        // Arrange
        var orderQueue = CreateMockOrderQueue();
        await _repository.AddAsync(orderQueue, CancellationToken.None);

        // Act
        await _repository.UpdateStatusAsync(orderQueue.Id.Value, OrderQueueStatus.Completed, CancellationToken.None);
        var updatedOrder = await _repository.GetByIdAsync(orderQueue.Id.Value, CancellationToken.None);

        // Assert
        updatedOrder!.Status.Should().Be(OrderQueueStatus.Completed);
    }

    [Fact]
    public async Task GetByTransactionIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var transactionId = "TX123";

        // Act
        var result = await _repository.GetByTransactionIdAsync(transactionId, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByTransactionIdAsync_ShouldReturnOrder_WhenOrderExists()
    {
        // Arrange
        var orderQueue = CreateMockOrderQueue();
        await _repository.AddAsync(orderQueue, CancellationToken.None);

        // Act
        var result = await _repository.GetByTransactionIdAsync(orderQueue.TransactionId!, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TransactionId.Should().Be(orderQueue.TransactionId);
    }

    [Fact]
    public async Task CancelOrdersNotPreparingWithinAsync_ShouldCancelOrders()
    {
        // Arrange
        var oldOrder = CreateMockOrderQueue(DateTime.UtcNow.AddHours(-3), OrderQueueStatus.Received);
        var newOrder = CreateMockOrderQueue(DateTime.UtcNow, OrderQueueStatus.Received);

        await _repository.AddAsync(oldOrder, CancellationToken.None);
        await _repository.AddAsync(newOrder, CancellationToken.None);

        // Act
        await _repository.CancelOrdersNotPreparingWithinAsync(DateTime.UtcNow.AddHours(-2));
        var updatedOldOrder = await _repository.GetByIdAsync(oldOrder.Id.Value, CancellationToken.None);
        var updatedNewOrder = await _repository.GetByIdAsync(newOrder.Id.Value, CancellationToken.None);

        // Assert
        updatedOldOrder!.Status.Should().Be(OrderQueueStatus.Cancelled);
        updatedNewOrder!.Status.Should().Be(OrderQueueStatus.Received);
    }

    private static OrderQueue CreateMockOrderQueue(DateTime? createdAt = null,
        OrderQueueStatus status = OrderQueueStatus.Preparing)
    {
        var orderId = OrderId.New();
        var items = new List<OrderItem>
        {
            OrderItem.Create(OrderItemId.New(), orderId, new ProductId(Guid.NewGuid()), "Product 1", 10.99m, 2,
                ProductCategory.Acompanhamento),
            OrderItem.Create(OrderItemId.New(), orderId, new ProductId(Guid.NewGuid()), "Product 2", 20.49m, 1,
                ProductCategory.Lanche)
        };

        var orderQueue = OrderQueue.Create(orderId, Guid.NewGuid(), items, "TX123", status);

        // Simula a data de criação se fornecida
        if (createdAt.HasValue) typeof(OrderQueue).GetProperty("CreatedAt")!.SetValue(orderQueue, createdAt.Value);

        return orderQueue;
    }
}