namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectCostActual; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectCostActual : OrganizationEntity
{
    public ProjectCostActual(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid sourceDocumentId,
        string costCategory,
        decimal amount,
        string currency,
        DateOnly recordedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        CostCategory = DomainRules.Text(costCategory, nameof(costCategory));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        RecordedOn = DomainRules.Date(recordedOn, nameof(recordedOn));
    }

    public Guid ProjectId { get; }
    public Guid SourceDocumentId { get; }
    public string CostCategory { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public DateOnly RecordedOn { get; }
}
