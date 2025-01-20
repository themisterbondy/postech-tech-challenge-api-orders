namespace Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;

public class CreateOrderMessage
{
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public string TransactionId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}