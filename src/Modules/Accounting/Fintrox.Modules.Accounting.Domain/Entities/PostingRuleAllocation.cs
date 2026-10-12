namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped PostingRuleAllocation domain record; referenced objects are validated by application use cases.</summary>
public sealed class PostingRuleAllocation : OrganizationEntity
{
    public PostingRuleAllocation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid postingRuleVersionId,
        Guid accountId,
        string side,
        decimal percent,
        int sortOrder)
        : base(id, organizationId, createdAtUtc)
    {
        PostingRuleVersionId = DomainRules.Id(postingRuleVersionId, nameof(postingRuleVersionId));
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        Side = DomainRules.Text(side, nameof(side));
        Percent = DomainRules.Percent(percent, nameof(percent));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
    }

    public Guid PostingRuleVersionId { get; }
    public Guid AccountId { get; }
    public string Side { get; }
    public decimal Percent { get; }
    public int SortOrder { get; }
}
