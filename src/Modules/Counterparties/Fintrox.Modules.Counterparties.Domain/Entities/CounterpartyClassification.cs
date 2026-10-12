namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyClassification. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyClassification : OrganizationEntity
{
    public CounterpartyClassification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string classificationCode,
        DateOnly assignedOn,
        Guid assignedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        ClassificationCode = DomainRules.Text(classificationCode, nameof(classificationCode));
        AssignedOn = DomainRules.Date(assignedOn, nameof(assignedOn));
        AssignedByUserId = DomainRules.Id(assignedByUserId, nameof(assignedByUserId));
    }

    public Guid CounterpartyId { get; }
    public string ClassificationCode { get; }
    public DateOnly AssignedOn { get; }
    public Guid AssignedByUserId { get; }
}
