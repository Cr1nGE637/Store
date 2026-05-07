using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Store.Notifications.Application;
using Store.Notifications.Application.Interfaces;
using Store.Notifications.Infrastructure.Configuration;

namespace Store.Notifications.Infrastructure.Services;

public class EmailNotificationSender(
    IOptions<SmtpOptions> options,
    ILogger<EmailNotificationSender> logger) : INotificationSender
{
    private readonly SmtpOptions _options = options.Value;

    public async Task SendAsync(
        Guid outboxMessageId,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.MessageId = NotificationMessageId.Create(outboxMessageId);
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient { Timeout = _options.TimeoutSeconds * 1000 };
        var socketOptions = _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
        await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
        await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        logger.LogInformation(
            "Email sent to {To}: {Subject}. OutboxMessageId={OutboxMessageId}",
            to,
            subject,
            outboxMessageId);
    }
}
