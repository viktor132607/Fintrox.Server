namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceKnowledgeArticleVersion domain record. No cross-module navigation properties.</summary>
public sealed class ServiceKnowledgeArticleVersion : OrganizationEntity
{
    public ServiceKnowledgeArticleVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid articleId,
        int versionNumber,
        Guid changedByUserId,
        DateOnly publishedOn)
        : base(id, organizationId, createdAtUtc)
    {
        ArticleId = DomainRules.Id(articleId, nameof(articleId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        ChangedByUserId = DomainRules.Id(changedByUserId, nameof(changedByUserId));
        PublishedOn = DomainRules.Date(publishedOn, nameof(publishedOn));
    }

    public Guid ArticleId { get; }
    public int VersionNumber { get; }
    public Guid ChangedByUserId { get; }
    public DateOnly PublishedOn { get; }
}
