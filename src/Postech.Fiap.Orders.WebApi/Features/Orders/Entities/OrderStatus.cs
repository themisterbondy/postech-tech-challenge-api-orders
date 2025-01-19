namespace Postech.Fiap.Orders.WebApi.Features.Orders.Entities;

public enum OrderQueueStatus
{
    Received,
    Preparing,
    Ready,
    Completed,
    Cancelled
}