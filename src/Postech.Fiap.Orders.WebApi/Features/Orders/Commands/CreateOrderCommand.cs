using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Features.Products.Entities;
using Postech.Fiap.Orders.WebApi.Persistence;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Commands;

public abstract class CreateOrderCommand
{
    public class Command : IRequest<Result>
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public string TransactionId { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    public class Handler(ApplicationDbContext context, ILogger<Handler> logger) : IRequestHandler<Command, Result>
    {
        public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
        {
            var order = OrderQueue.Create(
                new OrderId(request.OrderId),
                request.CustomerId,
                request.Items.Select(i => OrderItem.Create(
                    OrderItemId.New(),
                    new OrderId(request.OrderId),
                    new ProductId(i.ProductId),
                    i.ProductName,
                    i.UnitPrice,
                    i.Quantity,
                    i.Category)).ToList(),
                request.TransactionId,
                OrderQueueStatus.Received);

            await context.OrderQueue.AddAsync(order, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Order {OrderId} created", request.OrderId);
            return Result.Success();
        }
    }
}