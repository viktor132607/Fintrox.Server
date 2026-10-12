namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceDemand model; no cross-module navigations.</summary>
public sealed class ResourceDemand : OrganizationEntity
{
    public ResourceDemand(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string skillCode,
        decimal requiredHours,
        DateOnly requiredFrom,
        DateOnly requiredUntil)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        SkillCode = DomainRules.Text(skillCode, nameof(skillCode));
        RequiredHours = DomainRules.Positive(requiredHours, nameof(requiredHours));
        RequiredFrom = DomainRules.Date(requiredFrom, nameof(requiredFrom));
        RequiredUntil = DomainRules.Date(requiredUntil, nameof(requiredUntil));
        if (RequiredUntil < RequiredFrom) throw new ArgumentException("End must not precede start.", nameof(requiredUntil));
    }

    public Guid ProjectId { get; }
    public string SkillCode { get; }
    public decimal RequiredHours { get; }
    public DateOnly RequiredFrom { get; }
    public DateOnly RequiredUntil { get; }
}
