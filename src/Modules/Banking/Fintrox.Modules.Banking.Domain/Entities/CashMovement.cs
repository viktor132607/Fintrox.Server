namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped CashMovement record; cross-module references require application authorization.</summary>
public sealed class CashMovement : OrganizationEntity
{
    public CashMovement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cashAccountId,
        string reference,
        DateOnly movementOn,
        decimal amount,
        string currency,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        CashAccountId = DomainRules.Id(cashAccountId, nameof(cashAccountId));
        Reference = DomainRules.Text(reference, nameof(reference));
        MovementOn = DomainRules.Date(movementOn, nameof(movementOn));
        Amount = DomainRules.Nonzero(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid CashAccountId { get; }
    public string Reference { get; }
    public DateOnly MovementOn { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Reason { get; }
}
