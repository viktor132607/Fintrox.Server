namespace Fintrox.Modules.SupplyChain.Domain.Entities;

/// <summary>Tenant-scoped SupplyChainDisruption domain record. No cross-module navigation properties.</summary>
public sealed class SupplyChainDisruption : OrganizationEntity
{
    public SupplyChainDisruption(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid affectedSupplierId,
        string disruptionType,
        DateOnly startedOn,
        DateOnly expectedResolvedOn)
        : base(id, organizationId, createdAtUtc)
    {
        AffectedSupplierId = DomainRules.Id(affectedSupplierId, nameof(affectedSupplierId));
        DisruptionType = DomainRules.Text(disruptionType, nameof(disruptionType));
        StartedOn = DomainRules.Date(startedOn, nameof(startedOn));
        ExpectedResolvedOn = DomainRules.Date(expectedResolvedOn, nameof(expectedResolvedOn));
    }

    public Guid AffectedSupplierId { get; }
    public string DisruptionType { get; }
    public DateOnly StartedOn { get; }
    public DateOnly ExpectedResolvedOn { get; }
}
