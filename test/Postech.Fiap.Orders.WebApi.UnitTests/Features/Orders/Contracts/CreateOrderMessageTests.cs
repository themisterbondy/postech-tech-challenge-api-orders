using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Orders.Contracts;

public class CreateOrderMessageTests
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyItemsList()
    {
        // Act
        var message = new CreateOrderMessage();

        // Assert
        message.Items.Should().NotBeNull();
        message.Items.Should().BeEmpty();
    }

    [Fact]
    public void Properties_ShouldBeSetAndRetrievedCorrectly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var transactionId = "TX12345";
        var items = new List<OrderItemDto>
        {
            new() { ProductId = Guid.NewGuid(), Quantity = 2 },
            new() { ProductId = Guid.NewGuid(), Quantity = 5 }
        };

        // Act
        var message = new CreateOrderMessage
        {
            OrderId = orderId,
            CustomerId = customerId,
            TransactionId = transactionId,
            Items = items
        };

        // Assert
        message.OrderId.Should().Be(orderId);
        message.CustomerId.Should().Be(customerId);
        message.TransactionId.Should().Be(transactionId);
        message.Items.Should().BeEquivalentTo(items);
    }
}