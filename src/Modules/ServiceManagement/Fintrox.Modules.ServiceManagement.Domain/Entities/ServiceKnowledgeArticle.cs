namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceKnowledgeArticle domain record. No cross-module navigation properties.</summary>
public sealed class ServiceKnowledgeArticle : OrganizationEntity
{
    public ServiceKnowledgeArticle(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string articleCode,
        string title,
        string body,
        bool isPublished)
        : base(id, organizationId, createdAtUtc)
    {
        ArticleCode = DomainRules.Text(articleCode, nameof(articleCode));
        Title = DomainRules.Text(title, nameof(title));
        Body = DomainRules.Text(body, nameof(body));
        IsPublished = isPublished;
    }

    public string ArticleCode { get; }
    public string Title { get; }
    public string Body { get; }
    public bool IsPublished { get; }
}
