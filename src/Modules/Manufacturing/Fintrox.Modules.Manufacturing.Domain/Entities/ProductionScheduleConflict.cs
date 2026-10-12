namespace Fintrox.Modules.Manufacturing.Domain.Entities;

/// <summary>Immutable, organization-owned ProductionScheduleException record.</summary>
public sealed class ProductionScheduleConflict : OrganizationEntity
{
    public ProductionScheduleConflict(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid productionScheduleId,
        string exceptionCode,
        string description,
        DateTimeOffset detectedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        ProductionScheduleId = DomainRules.Id(productionScheduleId, nameof(productionScheduleId));
        ExceptionCode = DomainRules.Text(exceptionCode, nameof(exceptionCode));
        Description = DomainRules.Text(description, nameof(description));
        DetectedAtUtc = DomainRules.Instant(detectedAtUtc, nameof(detectedAtUtc));
    }

    public Guid ProductionScheduleId { get; }
    public string ExceptionCode { get; }
    public string Description { get; }
    public DateTimeOffset DetectedAtUtc { get; }
}
