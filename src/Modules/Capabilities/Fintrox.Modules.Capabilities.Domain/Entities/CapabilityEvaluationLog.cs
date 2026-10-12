namespace Fintrox.Modules.Capabilities.Domain.Entities;

/// <summary>Tenant-owned CapabilityEvaluationLog record. Domain invariants enforced at construction.</summary>
public sealed class CapabilityEvaluationLog : OrganizationEntity
{
    public CapabilityEvaluationLog(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid capabilityId,
        Guid principalId,
        DateTimeOffset evaluatedAtUtc,
        string outcome,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        CapabilityId = DomainRules.Id(capabilityId, nameof(capabilityId));
        PrincipalId = DomainRules.Id(principalId, nameof(principalId));
        EvaluatedAtUtc = DomainRules.Instant(evaluatedAtUtc, nameof(evaluatedAtUtc));
        Outcome = DomainRules.Text(outcome, nameof(outcome));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid CapabilityId { get; }
    public Guid PrincipalId { get; }
    public DateTimeOffset EvaluatedAtUtc { get; }
    public string Outcome { get; }
    public string Reason { get; }
}
