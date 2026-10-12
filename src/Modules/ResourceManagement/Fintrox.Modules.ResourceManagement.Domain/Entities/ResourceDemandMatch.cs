namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceDemandMatch model; no cross-module navigations.</summary>
public sealed class ResourceDemandMatch : OrganizationEntity
{
    public ResourceDemandMatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceDemandId,
        Guid resourceId,
        int matchScore,
        DateTimeOffset matchedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceDemandId = DomainRules.Id(resourceDemandId, nameof(resourceDemandId));
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        MatchScore = DomainRules.NonnegativeInt(matchScore, nameof(matchScore));
        MatchedAtUtc = DomainRules.Instant(matchedAtUtc, nameof(matchedAtUtc));
    }

    public Guid ResourceDemandId { get; }
    public Guid ResourceId { get; }
    public int MatchScore { get; }
    public DateTimeOffset MatchedAtUtc { get; }
}
