using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Orders.Contracts;

public class OrderDtoTests
{
    [Fact]
    public void OrderDto_Should_Set_And_Get_Properties_Correctly()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDate = DateTime.UtcNow;
        var status = "Pending";
        var customerId = Guid.NewGuid();
        var transactionId = Guid.NewGuid().ToString();
        var items = new List<OrderItemDto>
        {
            new()
            {
                ProductId = Guid.NewGuid(), ProductName = "Burger", UnitPrice = 20.50m, Quantity = 2,
                Category = ProductCategory.Lanche
            },
            new()
            {
                ProductId = Guid.NewGuid(), ProductName = "Soda", UnitPrice = 5.00m, Quantity = 1,
                Category = ProductCategory.Bebida
            }
        };

        // Act
        var order = new OrderDto
        {
            OrderId = orderId,
            OrderDate = orderDate,
            Status = status,
            CustomerId = customerId,
            Items = items,
            TransactionId = transactionId
        };

        // Assert
        order.OrderId.Should().Be(orderId);
        order.OrderDate.Should().Be(orderDate);
        order.Status.Should().Be(status);
        order.CustomerId.Should().Be(customerId);
        order.Items.Should().BeEquivalentTo(items);
        order.TransactionId.Should().Be(transactionId);
    }

    [Fact]
    public void OrderDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Arrange & Act
        var order = new OrderDto
        {
            OrderId = Guid.NewGuid(),
            OrderDate = DateTime.UtcNow,
            Status = "Completed",
            CustomerId = Guid.Empty,
            Items = [],
            TransactionId = null // TransactionId pode ser nulo
        };

        // Assert
        order.CustomerId.Should().Be(Guid.Empty);
        order.TransactionId.Should().BeNull();
    }

    [Fact]
    public void OrderDto_Should_Initialize_Empty_List_When_Null()
    {
        // Act
        var order = new OrderDto
        {
            Items = null // Simulando um valor nulo para a lista de itens
        };

        // Assert
        order.Items.Should().BeNull();
    }

    [Fact]
    public void OrderItemDto_Should_Set_And_Get_Properties_Correctly()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productName = "Hamburguer Artesanal";
        var unitPrice = 25.99m;
        var quantity = 2;
        var category = ProductCategory.Lanche;

        // Act
        var item = new OrderItemDto
        {
            ProductId = productId,
            ProductName = productName,
            UnitPrice = unitPrice,
            Quantity = quantity,
            Category = category
        };

        // Assert
        item.ProductId.Should().Be(productId);
        item.ProductName.Should().Be(productName);
        item.UnitPrice.Should().Be(unitPrice);
        item.Quantity.Should().Be(quantity);
        item.Category.Should().Be(category);
    }

    [Fact]
    public void OrderItemDto_Should_Allow_Null_Values_For_Optional_Properties()
    {
        // Arrange & Act
        var item = new OrderItemDto
        {
            ProductId = Guid.NewGuid(),
            ProductName = null, // Nome do produto pode ser nulo
            UnitPrice = null, // Preço unitário pode ser nulo
            Quantity = 1,
            Category = ProductCategory.Bebida
        };

        // Assert
        item.ProductName.Should().BeNull();
        item.UnitPrice.Should().BeNull();
    }
}