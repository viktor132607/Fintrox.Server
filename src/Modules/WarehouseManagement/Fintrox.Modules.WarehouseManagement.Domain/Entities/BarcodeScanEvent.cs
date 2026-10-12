namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned BarcodeScanEvent; tenant scope and input fields validated at creation.</summary>
public sealed class BarcodeScanEvent : OrganizationEntity
{
    public BarcodeScanEvent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string scannerCode,
        string barcode,
        DateTimeOffset scannedAtUtc,
        string scanType)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        ScannerCode = DomainRules.Text(scannerCode, nameof(scannerCode));
        Barcode = DomainRules.Text(barcode, nameof(barcode));
        ScannedAtUtc = DomainRules.Instant(scannedAtUtc, nameof(scannedAtUtc));
        ScanType = DomainRules.Text(scanType, nameof(scanType));
    }

    public Guid WarehouseId { get; }
    public string ScannerCode { get; }
    public string Barcode { get; }
    public DateTimeOffset ScannedAtUtc { get; }
    public string ScanType { get; }
}
