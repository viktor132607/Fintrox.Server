namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned MachineDowntime record.</summary>
public sealed class MachineDowntime : OrganizationEntity
{
    public MachineDowntime(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid machineAssetId,
        DateTimeOffset startedAtUtc,
        DateTimeOffset endedAtUtc,
        string reasonCode)
        : base(id, organizationId, createdAtUtc)
    {
        MachineAssetId = DomainRules.Id(machineAssetId, nameof(machineAssetId));
        StartedAtUtc = DomainRules.Instant(startedAtUtc, nameof(startedAtUtc));
        EndedAtUtc = DomainRules.Instant(endedAtUtc, nameof(endedAtUtc));
        ReasonCode = DomainRules.Text(reasonCode, nameof(reasonCode));
    }

    public Guid MachineAssetId { get; }
    public DateTimeOffset StartedAtUtc { get; }
    public DateTimeOffset EndedAtUtc { get; }
    public string ReasonCode { get; }
}
