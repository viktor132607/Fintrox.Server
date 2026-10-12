namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesOrderStatusHistory model; no cross-module navigations.</summary>
public sealed class SalesOrderStatusHistory : OrganizationEntity
{
    public SalesOrderStatusHistory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesOrderId,
        string previousStatus,
        string newStatus,
        Guid changedByUserId,
        DateTimeOffset changedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SalesOrderId = DomainRules.Id(salesOrderId, nameof(salesOrderId));
        PreviousStatus = DomainRules.Text(previousStatus, nameof(previousStatus));
        NewStatus = DomainRules.Text(newStatus, nameof(newStatus));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
        ChangedAtUtc = DomainRules.Instant(changedAtUtc, nameof(changedAtUtc));
    }

    public Guid SalesOrderId { get; }
    public string PreviousStatus { get; }
    public string NewStatus { get; }
    public Guid ChangedByUserId { get; }
    public DateTimeOffset ChangedAtUtc { get; }
}
