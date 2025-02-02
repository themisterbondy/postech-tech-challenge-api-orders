using Postech.Fiap.Orders.WebApi.Common.Messaging;
using Postech.Fiap.Orders.WebApi.Features.Orders.Commands;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Messaging.Queues;
using Quartz;

namespace Postech.Fiap.Orders.WebApi.Features.Orders.Jobs;

[ExcludeFromCodeCoverage]
public class CreateOrderCommandSubmittedJob(
    ICreateOrderCommandSubmittedQueueClient queueClient,
    ISender sender,
    IConfiguration configuration,
    ILogger<CreateOrderCommandSubmittedJob> logger)
    : IJob

{
    private int _retryLimit;

    public async Task Execute(IJobExecutionContext context)
    {
        var jobName = context.JobDetail.Key.Name;

        if (!int.TryParse(configuration["RetryLimit"], out _retryLimit)) _retryLimit = 5;

        var messageResult =
            await queueClient.ReceiveAsync<CreateOrderMessage>(context.CancellationToken);

        if (messageResult.IsFailure)
        {
            logger.LogError(
                "Job {JobName}: Falha ao receber a mensagem - {ErrorCode} - {ErrorMessage}",
                jobName,
                messageResult.Error.Code,
                messageResult.Error.Message);
            return;
        }

        if (messageResult.Value == null) return;

        var message = messageResult.Value;

        if (message.MessageDetails.DequeueCount >= _retryLimit)
        {
            logger.LogError(
                "Job {JobName}: Mensagem para Order {OrderId} excedeu o limite de {RetryLimit} tentativas",
                jobName,
                message.MessageContent.OrderId,
                _retryLimit);

            await HandleExceededRetryLimitAsync(message, jobName, context.CancellationToken);
            return;
        }

        if (message.MessageContent == null || message.MessageDetails == null)
            logger.LogError(
                "Job {JobName}: Falha na validação da mensagem para Order {OrderId}",
                jobName,
                message.MessageContent.OrderId);

        var command = new CreateOrderCommand.Command
        {
            OrderId = message.MessageContent.OrderId,
            CustomerId = message.MessageContent.CustomerId,
            TransactionId = message.MessageContent.TransactionId,
            Items = message.MessageContent.Items
        };


        var result = await sender.Send(command, context.CancellationToken);

        if (result.IsFailure)
            logger.LogError(
                "Job {JobName}: Falha ao processar a mensagem para Order {OrderId} - {ErrorCode} - {ErrorMessage}",
                jobName,
                message.MessageContent.OrderId,
                result.Error.Code,
                result.Error.Message);

        await DeleteMessageWithRetryAsync(message, jobName, context.CancellationToken);
    }

    private async Task HandleExceededRetryLimitAsync(QueueMessagePayload<CreateOrderMessage> message,
        string jobName, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Job {JobName}: Deletando mensagem para Order {OrderId} e enviando evento de erro",
            jobName,
            message.MessageContent.OrderId);

        await DeleteMessageWithRetryAsync(message, jobName, cancellationToken);
    }


    private async Task DeleteMessageWithRetryAsync(QueueMessagePayload<CreateOrderMessage> message,
        string jobName, CancellationToken cancellationToken)
    {
        var deleteResult = await queueClient.DeleteAsync(message.MessageDetails.MessageId,
            message.MessageDetails.PopReceipt, cancellationToken);

        if (deleteResult.IsFailure)
            logger.LogError(
                "Job {JobName}: Falha ao deletar a mensagem para Order {OrderId} - {ErrorCode} - {ErrorMessage} MessageId: {MessageId}",
                jobName,
                message.MessageContent.OrderId,
                deleteResult.Error.Code,
                deleteResult.Error.Message,
                message.MessageDetails.MessageId
            );
    }
}