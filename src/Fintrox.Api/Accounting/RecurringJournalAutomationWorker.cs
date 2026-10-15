using Fintrox.Application.Accounting;

namespace Fintrox.Api.Accounting;

/// <summary>
/// UTC daily recurring journal automation; generates drafts only when users
/// explicitly enabled each template. The DB lease coordinates all API hosts.
/// </summary>
public sealed class RecurringJournalAutomationWorker(
    IServiceScopeFactory scopeFactory,
    TimeProvider clock,
    IConfiguration configuration,
    ILogger<RecurringJournalAutomationWorker> logger) : BackgroundService
{
    private static readonly Action<ILogger, Exception?> LogFailure =
        LoggerMessage.Define(
            LogLevel.Error,
            new EventId(3101, nameof(RecurringJournalAutomationWorker)),
            "Recurring journal automation batch failed.");

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Test hosts can disable the polling loop and invoke ProcessDueAsync
        // explicitly to test deterministic historical posting dates.
        if (!configuration.GetValue("RecurringJournals:EnableWorker", true))
            return;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var service = scope.ServiceProvider
                    .GetRequiredService<IRecurringJournalAutomationService>();
                var utcDate = DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime);
                await service.ProcessDueAsync(utcDate, 10, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                LogFailure(logger, ex);
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), clock, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
