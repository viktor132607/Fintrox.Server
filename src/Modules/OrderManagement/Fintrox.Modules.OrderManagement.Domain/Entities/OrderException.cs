namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned OrderException record.</summary>
public sealed class OrderException : OrganizationEntity
{
    public OrderException(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fulfillmentOrderId,
        string exceptionCode,
        string reason,
        DateTimeOffset detectedAtUtc,
        string severity)
        : base(id, organizationId, createdAtUtc)
    {
        FulfillmentOrderId = DomainRules.Id(fulfillmentOrderId, nameof(fulfillmentOrderId));
        ExceptionCode = DomainRules.Text(exceptionCode, nameof(exceptionCode));
        Reason = DomainRules.Text(reason, nameof(reason));
        DetectedAtUtc = DomainRules.Instant(detectedAtUtc, nameof(detectedAtUtc));
        Severity = DomainRules.Text(severity, nameof(severity));
    }

    public Guid FulfillmentOrderId { get; }
    public string ExceptionCode { get; }
    public string Reason { get; }
    public DateTimeOffset DetectedAtUtc { get; }
    public string Severity { get; }
}
