namespace Store.Notifications.Infrastructure.Configuration;

public class SmtpOptions
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public bool UseSsl { get; set; } = false;
    public int TimeoutSeconds { get; set; } = 30;
    public int OutboxPollingIntervalSeconds { get; set; } = 15;
    public int OutboxMaxAttempts { get; set; } = 5;
    public int OutboxBackoffBaseSeconds { get; set; } = 30;
}
