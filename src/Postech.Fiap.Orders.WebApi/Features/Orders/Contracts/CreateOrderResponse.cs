using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;

public class EnqueueOrderResponse
{
    public Guid OrderId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CustomerId { get; set; }
    public OrderQueueStatus Status { get; set; }
    public List<OrderItemDto> Items { get; set; }
    public string? TransactionId { get; set; }
}

public class ListOrdersResponse
{
    public List<OrderDto> Orders { get; set; }
}