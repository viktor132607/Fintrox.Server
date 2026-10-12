namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyDocumentRequirement. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyDocumentRequirement : OrganizationEntity
{
    public CounterpartyDocumentRequirement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string documentType,
        DateOnly dueOn,
        bool isMandatory,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        IsMandatory = isMandatory;
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid CounterpartyId { get; }
    public string DocumentType { get; }
    public DateOnly DueOn { get; }
    public bool IsMandatory { get; }
    public string Status { get; }
}
