namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped CashAccount record; cross-module references require application authorization.</summary>
public sealed class CashAccount : OrganizationEntity
{
    public CashAccount(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string currency,
        Guid custodianUserId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Currency = DomainRules.Currency(currency, nameof(currency));
        CustodianUserId = DomainRules.Id(custodianUserId, nameof(custodianUserId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public string Currency { get; }
    public Guid CustodianUserId { get; }
    public bool IsActive { get; }
}
