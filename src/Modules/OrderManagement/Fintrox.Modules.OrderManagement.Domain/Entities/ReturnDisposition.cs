namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned ReturnDisposition record.</summary>
public sealed class ReturnDisposition : OrganizationEntity
{
    public ReturnDisposition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid returnLineId,
        string dispositionCode,
        decimal refundAmount,
        string currency,
        Guid approvedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        ReturnLineId = DomainRules.Id(returnLineId, nameof(returnLineId));
        DispositionCode = DomainRules.Text(dispositionCode, nameof(dispositionCode));
        RefundAmount = DomainRules.Nonnegative(refundAmount, nameof(refundAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ApprovedByUserId = DomainRules.Id(approvedByUserId, nameof(approvedByUserId));
    }

    public Guid ReturnLineId { get; }
    public string DispositionCode { get; }
    public decimal RefundAmount { get; }
    public string Currency { get; }
    public Guid ApprovedByUserId { get; }
}
