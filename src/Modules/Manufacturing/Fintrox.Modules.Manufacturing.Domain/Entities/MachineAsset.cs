namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned MachineAsset record.</summary>
public sealed class MachineAsset : OrganizationEntity
{
    public MachineAsset(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid workCenterId,
        Guid fixedAssetId,
        string machineCode,
        bool isOperational)
        : base(id, organizationId, createdAtUtc)
    {
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        MachineCode = DomainRules.Text(machineCode, nameof(machineCode));
        IsOperational = isOperational;
    }

    public Guid WorkCenterId { get; }
    public Guid FixedAssetId { get; }
    public string MachineCode { get; }
    public bool IsOperational { get; }
}
