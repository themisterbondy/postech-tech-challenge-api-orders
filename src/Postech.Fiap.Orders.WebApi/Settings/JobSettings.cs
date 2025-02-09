namespace Postech.Fiap.Orders.WebApi.Settings;

[ExcludeFromCodeCoverage]
public class JobSettings
{
    public Job CreateOrderCommandSubmittedJob { get; init; }
}

[ExcludeFromCodeCoverage]
public class Job
{
    public string JobName { get; init; }
    public string TriggerName { get; init; }
    public string TriggerGroup { get; init; }
    public string CronExpression { get; init; }
}