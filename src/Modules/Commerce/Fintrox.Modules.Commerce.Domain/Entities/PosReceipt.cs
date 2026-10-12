namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PosReceipt; external references validated by application layer.</summary>
public sealed class PosReceipt : OrganizationEntity
{
    public PosReceipt(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid posSessionId,
        string receiptNumber,
        DateTimeOffset issuedAtUtc,
        decimal totalAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        PosSessionId = DomainRules.Id(posSessionId, nameof(posSessionId));
        ReceiptNumber = DomainRules.Text(receiptNumber, nameof(receiptNumber));
        IssuedAtUtc = DomainRules.Instant(issuedAtUtc, nameof(issuedAtUtc));
        TotalAmount = DomainRules.Nonnegative(totalAmount, nameof(totalAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid PosSessionId { get; }
    public string ReceiptNumber { get; }
    public DateTimeOffset IssuedAtUtc { get; }
    public decimal TotalAmount { get; }
    public string Currency { get; }
}
