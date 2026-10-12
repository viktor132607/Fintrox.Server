namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountHierarchyNode domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountHierarchyNode : OrganizationEntity
{
    public AccountHierarchyNode(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountId,
        Guid parentAccountId,
        int sortOrder,
        int level)
        : base(id, organizationId, createdAtUtc)
    {
        AccountId = DomainRules.Id(accountId, nameof(accountId));
        ParentAccountId = DomainRules.Id(parentAccountId, nameof(parentAccountId));
        SortOrder = DomainRules.NonnegativeInt(sortOrder, nameof(sortOrder));
        Level = DomainRules.PositiveInt(level, nameof(level));
    }

    public Guid AccountId { get; }
    public Guid ParentAccountId { get; }
    public int SortOrder { get; }
    public int Level { get; }
}
