namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned WarehouseAutomationCommand; tenant scope and input fields validated at creation.</summary>
public sealed class WarehouseAutomationCommand : OrganizationEntity
{
    public WarehouseAutomationCommand(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid warehouseId,
        string commandType,
        string payloadJson,
        DateTimeOffset requestedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        WarehouseId = DomainRules.Id(warehouseId, nameof(warehouseId));
        CommandType = DomainRules.Text(commandType, nameof(commandType));
        PayloadJson = DomainRules.Json(payloadJson, nameof(payloadJson));
        RequestedAtUtc = DomainRules.Instant(requestedAtUtc, nameof(requestedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid WarehouseId { get; }
    public string CommandType { get; }
    public string PayloadJson { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public string Status { get; }
}
