namespace Fintrox.Modules.Projects.Domain.Entities;

public sealed class ProjectMilestone : OrganizationEntity
{
    public ProjectMilestone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        string name,
        DateOnly dueOn,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        Name = DomainRules.Text(name, nameof(name));
        DueOn = DomainRules.Date(dueOn, nameof(dueOn));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProjectId { get; }
    public string Name { get; }
    public DateOnly DueOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
