namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped FieldServiceVisit domain record. No cross-module navigation properties.</summary>
public sealed class FieldServiceVisit : OrganizationEntity
{
    public FieldServiceVisit(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fieldServiceOrderId,
        Guid technicianId,
        DateTimeOffset startsAtUtc,
        DateTimeOffset endsAtUtc,
        string visitStatus)
        : base(id, organizationId, createdAtUtc)
    {
        FieldServiceOrderId = DomainRules.Id(fieldServiceOrderId, nameof(fieldServiceOrderId));
        TechnicianId = DomainRules.Id(technicianId, nameof(technicianId));
        StartsAtUtc = DomainRules.Instant(startsAtUtc, nameof(startsAtUtc));
        EndsAtUtc = DomainRules.Instant(endsAtUtc, nameof(endsAtUtc));
        VisitStatus = DomainRules.Text(visitStatus, nameof(visitStatus));
    }

    public Guid FieldServiceOrderId { get; }
    public Guid TechnicianId { get; }
    public DateTimeOffset StartsAtUtc { get; }
    public DateTimeOffset EndsAtUtc { get; }
    public string VisitStatus { get; }
}
