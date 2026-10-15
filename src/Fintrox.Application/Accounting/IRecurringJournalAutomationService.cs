using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IRecurringJournalAutomationService
{
    Task<RecurringJournalAutomationResponse?> ConfigureAsync(
        Guid templateId, ConfigureRecurringJournalAutomationRequest request,
        CancellationToken cancellationToken);

    Task<RecurringJournalAutomationResponse?> GetConfigurationAsync(
        Guid templateId, CancellationToken cancellationToken);

    Task<IReadOnlyList<RecurringJournalAutomationRunResponse>?> GetRunsAsync(
        Guid templateId, CancellationToken cancellationToken);

    /// <summary>Worker-only operation; never reachable by a public HTTP route.</summary>
    Task<int> ProcessDueAsync(DateOnly utcDate, int maxItems, CancellationToken cancellationToken);
}
