using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Entities;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Services;

public interface IOrderQueueService
{
    Task<Result<EnqueueOrderResponse>> GetOrderByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<EnqueueOrderResponse>> UpdateOrderStatusAsync(Guid id, OrderQueueStatus status,
        CancellationToken cancellationToken);

    Task<Result<EnqueueOrderResponse>> GetOrderByTransactionIdAsync(string TransactionId,
        CancellationToken cancellationToken);
}