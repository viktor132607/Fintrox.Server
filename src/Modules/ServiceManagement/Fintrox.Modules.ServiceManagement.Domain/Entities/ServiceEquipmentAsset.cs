namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceEquipmentAsset domain record. No cross-module navigation properties.</summary>
public sealed class ServiceEquipmentAsset : OrganizationEntity
{
    public ServiceEquipmentAsset(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        Guid fixedAssetId,
        string serialNumber,
        DateOnly installedOn,
        bool isUnderWarranty)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        SerialNumber = DomainRules.Text(serialNumber, nameof(serialNumber));
        InstalledOn = DomainRules.Date(installedOn, nameof(installedOn));
        IsUnderWarranty = isUnderWarranty;
    }

    public Guid CounterpartyId { get; }
    public Guid FixedAssetId { get; }
    public string SerialNumber { get; }
    public DateOnly InstalledOn { get; }
    public bool IsUnderWarranty { get; }
}
