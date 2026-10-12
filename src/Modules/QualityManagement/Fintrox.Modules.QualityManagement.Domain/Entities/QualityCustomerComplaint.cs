namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityCustomerComplaint model. References require application validation.</summary>
public sealed class QualityCustomerComplaint : OrganizationEntity
{
    public QualityCustomerComplaint(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid sourceOrderId,
        string complaintReference,
        DateOnly receivedOn,
        string severity)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        SourceOrderId = DomainRules.Id(sourceOrderId, nameof(sourceOrderId));
        ComplaintReference = DomainRules.Text(complaintReference, nameof(complaintReference));
        ReceivedOn = DomainRules.Date(receivedOn, nameof(receivedOn));
        Severity = DomainRules.Text(severity, nameof(severity));
    }

    public Guid CounterpartyId { get; }
    public Guid SourceOrderId { get; }
    public string ComplaintReference { get; }
    public DateOnly ReceivedOn { get; }
    public string Severity { get; }
}
