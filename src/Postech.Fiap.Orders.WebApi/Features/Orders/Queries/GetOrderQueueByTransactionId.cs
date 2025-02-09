using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Queries;

public class GetOrderQueueByTransactionId
{
    public class Query : IRequest<Result<EnqueueOrderResponse>>
    {
        public string TransactionId { get; set; }
    }

    public class Handler(IOrderQueueService orderQueueService) : IRequestHandler<Query, Result<EnqueueOrderResponse>>
    {
        public async Task<Result<EnqueueOrderResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await orderQueueService.GetOrderByTransactionIdAsync(request.TransactionId, cancellationToken);
        }
    }
}