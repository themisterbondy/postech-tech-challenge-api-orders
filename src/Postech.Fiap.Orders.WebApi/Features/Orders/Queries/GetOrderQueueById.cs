using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Services;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Queries;

public class GetOrderQueueById
{
    public class Query : IRequest<Result<EnqueueOrderResponse>>
    {
        public Guid Id { get; set; }
    }

    public class Handler(IOrderQueueService orderQueueService) : IRequestHandler<Query, Result<EnqueueOrderResponse>>
    {
        public async Task<Result<EnqueueOrderResponse>> Handle(Query request, CancellationToken cancellationToken)
        {
            return await orderQueueService.GetOrderByIdAsync(request.Id, cancellationToken);
        }
    }
}