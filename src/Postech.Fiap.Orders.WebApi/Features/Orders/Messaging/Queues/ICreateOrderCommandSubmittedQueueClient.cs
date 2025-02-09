using Postech.Fiap.Orders.WebApi.Common.Messaging;
using Postech.Fiap.Orders.WebApi.Common.ResultPattern;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Messaging.Queues;

public interface ICreateOrderCommandSubmittedQueueClient
{
    Task PublishAsync<T>(T message, CancellationToken cancellationToken);
    Task<Result<QueueMessagePayload<T>>> ReceiveAsync<T>(CancellationToken cancellationToken);
    Task<Result<List<QueueMessagePayload<T>>>> ReceiveAllAsync<T>(CancellationToken cancellationToken);

    Task<Result<QueueMessagePayload<T>>> UpdateAsync<T>(T message, string messageId, string popReceipt,
        CancellationToken cancellationToken);

    Task<Result> DeleteAsync(string messageId, string popReceipt, CancellationToken cancellationToken);
}