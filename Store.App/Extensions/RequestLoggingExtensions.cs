using System.Diagnostics;
using System.Security.Claims;

namespace Store.App.Extensions;

public static class RequestLoggingExtensions
{
    private const string TraceHeaderName = "X-Trace-Id";
    private static readonly PathString HealthPath = new("/health");

    public static IApplicationBuilder UseStoreRequestLogging(this IApplicationBuilder app)
    {
        var logger = app.ApplicationServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("Store.App.Requests");

        return app.Use(async (context, next) =>
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            context.Response.OnStarting(() =>
            {
                context.Response.Headers.TryAdd(TraceHeaderName, traceId);
                return Task.CompletedTask;
            });

            var start = Stopwatch.GetTimestamp();

            try
            {
                await next(context);

                LogRequest(
                    logger,
                    context,
                    traceId,
                    Stopwatch.GetElapsedTime(start),
                    exception: null);
            }
            catch (Exception exception)
            {
                LogRequest(
                    logger,
                    context,
                    traceId,
                    Stopwatch.GetElapsedTime(start),
                    exception);

                throw;
            }
        });
    }

    private static void LogRequest(
        ILogger logger,
        HttpContext context,
        string traceId,
        TimeSpan elapsed,
        Exception? exception)
    {
        if (context.Request.Path.Equals(HealthPath))
        {
            return;
        }

        var statusCode = exception is null
            ? context.Response.StatusCode
            : StatusCodes.Status500InternalServerError;

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var elapsedMs = elapsed.TotalMilliseconds;

        if (exception is not null)
        {
            logger.LogError(
                exception,
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0000} ms TraceId={TraceId} UserId={UserId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsedMs,
                traceId,
                userId);

            return;
        }

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0000} ms TraceId={TraceId} UserId={UserId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsedMs,
                traceId,
                userId);

            return;
        }

        if (statusCode >= StatusCodes.Status400BadRequest)
        {
            logger.LogWarning(
                "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0000} ms TraceId={TraceId} UserId={UserId}",
                context.Request.Method,
                context.Request.Path.Value,
                statusCode,
                elapsedMs,
                traceId,
                userId);

            return;
        }

        logger.LogInformation(
            "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds:0.0000} ms TraceId={TraceId} UserId={UserId}",
            context.Request.Method,
            context.Request.Path.Value,
            statusCode,
            elapsedMs,
            traceId,
            userId);
    }
}
