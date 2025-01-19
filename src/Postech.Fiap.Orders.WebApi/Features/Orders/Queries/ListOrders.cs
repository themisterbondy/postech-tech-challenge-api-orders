using Microsoft.EntityFrameworkCore;
using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;
using Postech.Fiap.Orders.WebApi.Persistence;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Queries;

public class ListOrders
{
    public class Query : IRequest<Result<ListOrdersResponse>>;

    public class Handler(ApplicationDbContext context) : IRequestHandler<Query, Result<ListOrdersResponse>>
    {
        public async Task<Result<ListOrdersResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            var orders = await context.OrderQueue
                .Include(o => o.Items)
                .Where(o => o.Status != OrderQueueStatus.Cancelled && o.Status != OrderQueueStatus.Completed)
                .OrderBy(o => o.Status == OrderQueueStatus.Ready ? 0
                    : o.Status == OrderQueueStatus.Preparing ? 1
                    : 2)
                .ThenBy(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            return Result.Success(new ListOrdersResponse
            {
                Orders = orders.Select(o => new OrderDto
                {
                    OrderId = o.Id.Value,
                    OrderDate = o.CreatedAt,
                    Status = o.Status.ToString(),
                    CustomerId = o.CustomerId,
                    TransactionId = o.TransactionId,
                    Items = o.Items.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId.Value,
                        ProductName = oi.ProductName,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        Category = oi.Category
                    }).ToList()
                }).ToList()
            });
        }
    }
}