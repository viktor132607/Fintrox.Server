namespace Fintrox.Modules.ContractManagement.Domain.Entities;

/// <summary>Tenant-owned ContractClause; external references validated by application layer.</summary>
public sealed class ContractClause : OrganizationEntity
{
    public ContractClause(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid contractVersionId,
        string clauseNumber,
        string title,
        string content,
        bool isMandatory)
        : base(id, organizationId, createdAtUtc)
    {
        ContractVersionId = DomainRules.Id(contractVersionId, nameof(contractVersionId));
        ClauseNumber = DomainRules.Text(clauseNumber, nameof(clauseNumber));
        Title = DomainRules.Text(title, nameof(title));
        Content = DomainRules.Text(content, nameof(content));
        IsMandatory = isMandatory;
    }

    public Guid ContractVersionId { get; }
    public string ClauseNumber { get; }
    public string Title { get; }
    public string Content { get; }
    public bool IsMandatory { get; }
}
