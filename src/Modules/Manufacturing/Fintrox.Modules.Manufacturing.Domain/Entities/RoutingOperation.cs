namespace Fintrox.Modules.Manufacturing.Domain.Entities;

public sealed class RoutingOperation : OrganizationEntity
{
    public RoutingOperation(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid routingId,
        int sequence,
        Guid workCenterId,
        decimal durationMinutes)
        : base(id, organizationId, createdAtUtc)
    {
        RoutingId = DomainRules.Id(routingId, nameof(routingId));
        Sequence = DomainRules.PositiveInt(sequence, nameof(sequence));
        WorkCenterId = DomainRules.Id(workCenterId, nameof(workCenterId));
        DurationMinutes = DomainRules.Positive(durationMinutes, nameof(durationMinutes));
    }

    public Guid RoutingId { get; }
    public int Sequence { get; }
    public Guid WorkCenterId { get; }
    public decimal DurationMinutes { get; }
}
