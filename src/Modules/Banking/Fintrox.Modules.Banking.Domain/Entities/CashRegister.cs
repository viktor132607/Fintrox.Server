namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped CashRegister record; cross-module references require application authorization.</summary>
public sealed class CashRegister : OrganizationEntity
{
    public CashRegister(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid cashAccountId,
        string locationCode,
        decimal openingAmount,
        string currency,
        DateTimeOffset openedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        CashAccountId = DomainRules.Id(cashAccountId, nameof(cashAccountId));
        LocationCode = DomainRules.Text(locationCode, nameof(locationCode));
        OpeningAmount = DomainRules.Nonnegative(openingAmount, nameof(openingAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        OpenedAtUtc = DomainRules.Instant(openedAtUtc, nameof(openedAtUtc));
    }

    public Guid CashAccountId { get; }
    public string LocationCode { get; }
    public decimal OpeningAmount { get; }
    public string Currency { get; }
    public DateTimeOffset OpenedAtUtc { get; }
}
