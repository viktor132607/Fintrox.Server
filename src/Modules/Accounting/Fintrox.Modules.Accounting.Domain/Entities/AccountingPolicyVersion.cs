namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountingPolicyVersion domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountingPolicyVersion : OrganizationEntity
{
    public AccountingPolicyVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        int version,
        DateOnly effectiveFrom,
        string policyJson,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Version = DomainRules.PositiveInt(version, nameof(version));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        PolicyJson = DomainRules.Json(policyJson, nameof(policyJson));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public string Code { get; }
    public int Version { get; }
    public DateOnly EffectiveFrom { get; }
    public string PolicyJson { get; }
    public Guid ApprovedByUserId { get; }
}
