using Postech.Fiap.Orders.WebApi.Features.Products.Entities;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Products.Entities;

public class ProductIdTests
{
    [Fact]
    public void New_ShouldCreateNewGuid()
    {
        // Act
        var productId1 = ProductId.New();
        var productId2 = ProductId.New();

        // Assert
        productId1.Value.Should().NotBeEmpty();
        productId2.Value.Should().NotBeEmpty();
        productId1.Should().NotBe(productId2); // Cada chamado de `New()` deve gerar um ID diferente
    }

    [Fact]
    public void Constructor_ShouldAssignValue()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var productId = new ProductId(guid);

        // Assert
        productId.Value.Should().Be(guid);
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameGuid()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var productId1 = new ProductId(guid);
        var productId2 = new ProductId(guid);

        // Act & Assert
        productId1.Should().Be(productId2);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentGuids()
    {
        // Arrange
        var productId1 = new ProductId(Guid.NewGuid());
        var productId2 = new ProductId(Guid.NewGuid());

        // Act & Assert
        productId1.Should().NotBe(productId2);
    }
}