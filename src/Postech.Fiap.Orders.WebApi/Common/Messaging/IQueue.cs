using Postech.Fiap.Orders.WebApi.Common.ResultPattern;

namespace Postech.Fiap.Orders.WebApi.Common.Messaging;

public interface IQueue
{
    Task<Result> PublishMessageAsync<T>(
        string queueName,
        T message,
        CancellationToken cancellationToken = default);

    Task<Result<QueueMessagePayload<T>>> ReceiveMessageAsync<T>(string queueName,
        CancellationToken cancellationToken = default);

    Task<Result<List<QueueMessagePayload<T>>>> ReceiveMessagesAsync<T>(string queueName,
        int maxMessages,
        CancellationToken cancellationToken = default);

    Task<Result<QueueMessagePayload<T>>> UpdateMessageAsync<T>(
        string queueName,
        T message,
        string messageId,
        string popReceipt,
        CancellationToken cancellationToken = default);

    Task<Result> DeleteMessageAsync(
        string queueName,
        string messageId,
        string popReceipt,
        CancellationToken cancellationToken = default);
}