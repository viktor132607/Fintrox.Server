namespace Fintrox.Modules.Projects.Domain.Entities;

public sealed class ProjectExpense : OrganizationEntity
{
    public ProjectExpense(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid sourceDocumentId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        SourceDocumentId = DomainRules.Id(sourceDocumentId, nameof(sourceDocumentId));
        Amount = DomainRules.Positive(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProjectId { get; }
    public Guid SourceDocumentId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
