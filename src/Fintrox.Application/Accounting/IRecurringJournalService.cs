using Fintrox.Contracts.Accounting;

namespace Fintrox.Application.Accounting;

public interface IRecurringJournalService
{
    Task<IReadOnlyList<RecurringJournalTemplateResponse>> ListAsync(
        CancellationToken cancellationToken);

    Task<RecurringJournalTemplateResponse> CreateAsync(
        CreateRecurringJournalTemplateRequest request,
        CancellationToken cancellationToken);

    Task<RecurringJournalGenerationResponse?> GenerateAsync(
        Guid templateId, GenerateRecurringJournalRequest request,
        CancellationToken cancellationToken);
}
