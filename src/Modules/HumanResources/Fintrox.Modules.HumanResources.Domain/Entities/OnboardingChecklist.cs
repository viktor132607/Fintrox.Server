namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned OnboardingChecklist domain model.</summary>
public sealed class OnboardingChecklist : OrganizationEntity
{
    public OnboardingChecklist(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid employeeId,
        string templateCode,
        DateOnly startedOn,
        DateOnly dueOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        EmployeeId = DomainRules.Id(employeeId, nameof(employeeId));
        TemplateCode = DomainRules.Text(templateCode, nameof(templateCode));
        StartedOn = DomainRules.Date(startedOn, nameof(startedOn));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid EmployeeId { get; }
    public string TemplateCode { get; }
    public DateOnly StartedOn { get; }
    public DateOnly DueOn { get; }
    public string Status { get; }
}
