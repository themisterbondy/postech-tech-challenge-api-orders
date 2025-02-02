using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Postech.Fiap.Orders.WebApi.Features.Orders.Commands;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;
using Postech.Fiap.Orders.WebApi.Persistence;
using Postech.Fiap.Orders.WepApi.UnitTests.Mocks;

namespace Postech.Fiap.Orders.WepApi.UnitTests.Features.Orders.Commands;

public class CreateOrderCommandHandlerTests
{
    private readonly ApplicationDbContext _context;
    private readonly CreateOrderCommand.Handler _handler;
    private readonly ILogger<CreateOrderCommand.Handler> _logger;

    public CreateOrderCommandHandlerTests()
    {
        _context = ApplicationDbContextMock.Create();
        _logger = Substitute.For<ILogger<CreateOrderCommand.Handler>>();
        _handler = new CreateOrderCommand.Handler(_context, _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateOrder_WhenRequestIsValid()
    {
        // Arrange
        var command = new CreateOrderCommand.Command
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TransactionId = "TX123",
            Items = new List<OrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(), ProductName = "Product 1", UnitPrice = 10.99m, Quantity = 2,
                    Category = ProductCategory.Acompanhamento
                },
                new()
                {
                    ProductId = Guid.NewGuid(), ProductName = "Product 2", UnitPrice = 5.99m, Quantity = 1,
                    Category = ProductCategory.Bebida
                }
            }
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var createdOrder = await _context.OrderQueue.FirstOrDefaultAsync(o => o.Id.Value == command.OrderId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        createdOrder.Should().NotBeNull();
        createdOrder!.CustomerId.Should().Be(command.CustomerId);
        createdOrder.TransactionId.Should().Be(command.TransactionId);
        createdOrder.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldLogInformation_WhenOrderIsCreated()
    {
        // Arrange
        var command = new CreateOrderCommand.Command
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TransactionId = "TX123",
            Items = new List<OrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(), ProductName = "Product 1", UnitPrice = 10.99m, Quantity = 2,
                    Category = ProductCategory.Acompanhamento
                }
            }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(v => v.ToString()!.Contains($"Order {command.OrderId} created")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenDatabaseFails()
    {
        // Arrange
        var command = new CreateOrderCommand.Command
        {
            OrderId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            TransactionId = "TX123",
            Items = new List<OrderItemDto>
            {
                new()
                {
                    ProductId = Guid.NewGuid(), ProductName = "Product 1", UnitPrice = 10.99m, Quantity = 2,
                    Category = ProductCategory.Acompanhamento
                }
            }
        };

        _context.Dispose(); // Simula uma falha no banco de dados

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}