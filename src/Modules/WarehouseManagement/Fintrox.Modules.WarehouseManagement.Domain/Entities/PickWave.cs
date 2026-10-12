namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned PickWave; tenant scope and input fields validated at creation.</summary>
public sealed class PickWave : OrganizationEntity
{
    public PickWave(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string waveReference,
        DateOnly scheduledOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        WaveReference = DomainRules.Text(waveReference, nameof(waveReference));
        ScheduledOn = DomainRules.Date(scheduledOn, nameof(scheduledOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public string WaveReference { get; }
    public DateOnly ScheduledOn { get; }
    public string Status { get; }
}
