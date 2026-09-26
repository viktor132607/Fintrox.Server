namespace Fintrox.Application.Integrations;

public sealed record IntegrationBusinessEventResult(
    string AggregateType,
    Guid AggregateId,
    Guid? JournalEntryId,
    string OutboxEventType,
    string PayloadJson);
