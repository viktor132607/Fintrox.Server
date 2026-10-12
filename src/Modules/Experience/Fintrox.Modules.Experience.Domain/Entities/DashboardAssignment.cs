namespace Fintrox.Modules.Experience.Domain.Entities;

/// <summary>Immutable organization-owned DashboardAssignment domain model.</summary>
public sealed class DashboardAssignment : OrganizationEntity
{
    public DashboardAssignment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid userId,
        Guid dashboardTemplateId,
        DateTimeOffset assignedAtUtc,
        bool isPrimary)
        : base(id, organizationId, createdAtUtc)
    {
        UserId = DomainRules.Id(userId, nameof(userId));
        DashboardTemplateId = DomainRules.Id(dashboardTemplateId, nameof(dashboardTemplateId));
        AssignedAtUtc = DomainRules.Instant(assignedAtUtc, nameof(assignedAtUtc));
        IsPrimary = isPrimary;
    }

    public Guid UserId { get; }
    public Guid DashboardTemplateId { get; }
    public DateTimeOffset AssignedAtUtc { get; }
    public bool IsPrimary { get; }
}
