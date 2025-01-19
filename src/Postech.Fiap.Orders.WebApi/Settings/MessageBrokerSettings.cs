namespace Postech.Fiap.Orders.WebApi.Settings;

public class MessageBrokerSettings
{
    public const string SettingsKey = "MessageBrokerSettings";

    public string Host { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}