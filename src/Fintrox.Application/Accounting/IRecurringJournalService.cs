using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IRecurringJournalService
{
    Task<IReadOnlyList<RecurringJournalTemplateResponse>> ListAsync(
        CancellationToken cancellationToken);

    Task<RecurringJournalTemplateResponse> CreateAsync(
        CreateRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken);

    Task<RecurringJournalScheduleResponse?> GetScheduleAsync(
        Guid templateId, DateOnly fromDate, DateOnly toDate,
        DateOnly? asOfDate, CancellationToken cancellationToken);

    Task<RecurringJournalRevisionResponse?> ReviseAsync(
        Guid previousTemplateId, ReviseRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken);

    Task<RecurringJournalLineageResponse?> GetLineageAsync(
        Guid templateId, CancellationToken cancellationToken);

    Task<RecurringJournalGenerationResponse?> GenerateAsync(
        Guid templateId, GenerateRecurringJournalRequest request,
        CancellationToken cancellationToken);
}
