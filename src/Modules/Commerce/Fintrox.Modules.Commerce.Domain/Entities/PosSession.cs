namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PosSession; external references validated by application layer.</summary>
public sealed class PosSession : OrganizationEntity
{
    public PosSession(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid terminalId,
        Guid cashierUserId,
        DateTimeOffset openedAtUtc,
        decimal openingCash,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        TerminalId = DomainRules.Id(terminalId, nameof(terminalId));
        CashierUserId = DomainRules.Id(cashierUserId, nameof(cashierUserId));
        OpenedAtUtc = DomainRules.Instant(openedAtUtc, nameof(openedAtUtc));
        OpeningCash = DomainRules.Nonnegative(openingCash, nameof(openingCash));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid TerminalId { get; }
    public Guid CashierUserId { get; }
    public DateTimeOffset OpenedAtUtc { get; }
    public decimal OpeningCash { get; }
    public string Currency { get; }
}
