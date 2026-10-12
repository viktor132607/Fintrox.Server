namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped ReplenishmentExecution domain record. No cross-module navigation properties.</summary>
public sealed class ReplenishmentExecution : OrganizationEntity
{
    public ReplenishmentExecution(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid replenishmentPolicyId,
        DateTimeOffset executedAtUtc,
        Guid createdDocumentId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ReplenishmentPolicyId = DomainRules.Id(replenishmentPolicyId, nameof(replenishmentPolicyId));
        ExecutedAtUtc = DomainRules.Instant(executedAtUtc, nameof(executedAtUtc));
        CreatedDocumentId = DomainRules.Id(createdDocumentId, nameof(createdDocumentId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ReplenishmentPolicyId { get; }
    public DateTimeOffset ExecutedAtUtc { get; }
    public Guid CreatedDocumentId { get; }
    public string Status { get; }
}
