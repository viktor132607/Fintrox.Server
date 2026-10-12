namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityConstraint record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityConstraint : OrganizationEntity
{
    public CapabilityConstraint(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        string constraintKey,
        string operator,
        string expectedValue,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        ConstraintKey = DomainRules.Text(constraintKey, nameof(constraintKey));
        Operator = DomainRules.Text(operator, nameof(operator));
        ExpectedValue = DomainRules.Text(expectedValue, nameof(expectedValue));
        IsRequired = isRequired;
    }

    public Guid CapabilityId { get; }
    public string ConstraintKey { get; }
    public string Operator { get; }
    public string ExpectedValue { get; }
    public bool IsRequired { get; }
}
