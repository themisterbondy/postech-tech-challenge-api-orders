using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Postech.Fiap.Orders.WebApi.Common.Messaging;
using Postech.Fiap.Orders.WebApi.Common.ResultPattern;
using Postech.Fiap.Orders.WebApi.Features.Orders.Commands;
using Postech.Fiap.Orders.WebApi.Features.Orders.Contracts;
using Postech.Fiap.Orders.WebApi.Features.Orders.Jobs;
using Postech.Fiap.Orders.WebApi.Features.Orders.Messaging.Queues;
using Quartz;

namespace Postech.Fiap.Orders.WebApi.UnitTests.Features.Orders.Jobs;

public class CreateOrderCommandSubmittedJobTests
{
    private readonly IConfiguration _configuration;
    private readonly CreateOrderCommandSubmittedJob _job;
    private readonly IJobExecutionContext _jobContext;
    private readonly ILogger<CreateOrderCommandSubmittedJob> _logger;
    private readonly ICreateOrderCommandSubmittedQueueClient _queueClient;
    private readonly ISender _sender;

    public CreateOrderCommandSubmittedJobTests()
    {
        _queueClient = Substitute.For<ICreateOrderCommandSubmittedQueueClient>();
        _sender = Substitute.For<ISender>();
        _configuration = Substitute.For<IConfiguration>();
        _logger = Substitute.For<ILogger<CreateOrderCommandSubmittedJob>>();
        _jobContext = Substitute.For<IJobExecutionContext>();

        _jobContext.JobDetail.Key.Returns(new JobKey("CreateOrderCommandSubmittedJob"));

        _configuration.GetSection("RetryLimit").Value.Returns("5");


        _job = new CreateOrderCommandSubmittedJob(_queueClient, _sender, _configuration, _logger);
    }

    [Fact]
    public async Task Execute_ShouldLogError_WhenQueueReceiveFails()
    {
        // Arrange
        _queueClient.ReceiveAsync<CreateOrderMessage>(Arg.Any<CancellationToken>())
            .Returns(Result.Failure<QueueMessagePayload<CreateOrderMessage>>(Error.Failure("Error", "Error message")));

        // Act
        await _job.Execute(_jobContext);

        // Assert
        _logger.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Falha ao receber a mensagem")),
            null,
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Execute_ShouldDoNothing_WhenMessageIsNull()
    {
        // Arrange
        _queueClient.ReceiveAsync<CreateOrderMessage>(Arg.Any<CancellationToken>())
            .Returns(Result.Success<QueueMessagePayload<CreateOrderMessage>>(null));

        // Act
        await _job.Execute(_jobContext);

        // Assert
        _logger.DidNotReceive().Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(), Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception, string>>());
    }

    [Fact]
    public async Task Execute_ShouldDeleteMessage_WhenRetryLimitExceeded()
    {
        // Arrange
        var message = CreateMockMessage(6); // Excede o limite de tentativas
        _queueClient.ReceiveAsync<CreateOrderMessage>(Arg.Any<CancellationToken>())
            .Returns(Result.Success(message));

        _queueClient.DeleteAsync(message.MessageDetails.MessageId, message.MessageDetails.PopReceipt,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _job.Execute(_jobContext);

        // Assert
        _queueClient.Received(1).DeleteAsync(message.MessageDetails.MessageId, message.MessageDetails.PopReceipt,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Execute_ShouldSendCommand_WhenMessageIsValid()
    {
        // Arrange
        var message = CreateMockMessage(1); // Dentro do limite de tentativas
        _queueClient.ReceiveAsync<CreateOrderMessage>(Arg.Any<CancellationToken>())
            .Returns(Result.Success(message));

        _queueClient.DeleteAsync(message.MessageDetails.MessageId, message.MessageDetails.PopReceipt,
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        _sender.Send(Arg.Any<CreateOrderCommand.Command>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _job.Execute(_jobContext);

        // Assert
        await _sender.Received(1).Send(Arg.Any<CreateOrderCommand.Command>(), Arg.Any<CancellationToken>());
    }

    private QueueMessagePayload<CreateOrderMessage> CreateMockMessage(int dequeueCount)
    {
        return new QueueMessagePayload<CreateOrderMessage>
        {
            MessageContent = new CreateOrderMessage
            {
                OrderId = Guid.NewGuid(),
                CustomerId = Guid.NewGuid(),
                TransactionId = "TX123",
                Items = new List<OrderItemDto>()
            },
            MessageDetails = new MessageDetails
            {
                MessageId = "msg-id",
                PopReceipt = "pop-receipt",
                MessageText = "Mock message text",
                Body = BinaryData.FromString("Mock message body"),
                NextVisibleOn = DateTimeOffset.UtcNow.AddMinutes(1),
                InsertedOn = DateTimeOffset.UtcNow,
                ExpiresOn = DateTimeOffset.UtcNow.AddDays(1),
                DequeueCount = dequeueCount
            }
        };
    }
}