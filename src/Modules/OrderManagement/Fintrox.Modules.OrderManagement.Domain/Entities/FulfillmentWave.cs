namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned FulfillmentWave record.</summary>
public sealed class FulfillmentWave : OrganizationEntity
{
    public FulfillmentWave(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string waveNumber,
        DateOnly scheduledOn,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        WaveNumber = DomainRules.Text(waveNumber, nameof(waveNumber));
        ScheduledOn = DomainRules.Date(scheduledOn, nameof(scheduledOn));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public string WaveNumber { get; }
    public DateOnly ScheduledOn { get; }
    public string Status { get; }
}
