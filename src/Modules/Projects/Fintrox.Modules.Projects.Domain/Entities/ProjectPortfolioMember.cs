namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectPortfolioMember; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectPortfolioMember : OrganizationEntity
{
    public ProjectPortfolioMember(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid portfolioId,
        Guid projectId,
        DateOnly joinedOn,
        int priority)
        : base(id, organizationId, createdAtUtc)
    {
        PortfolioId = DomainRules.Id(portfolioId, nameof(portfolioId));
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        JoinedOn = DomainRules.Date(joinedOn, nameof(joinedOn));
        Priority = DomainRules.NonnegativeInt(priority, nameof(priority));
    }

    public Guid PortfolioId { get; }
    public Guid ProjectId { get; }
    public DateOnly JoinedOn { get; }
    public int Priority { get; }
}
