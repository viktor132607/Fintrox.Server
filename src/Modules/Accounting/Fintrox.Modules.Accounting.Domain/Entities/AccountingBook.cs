namespace Fintrox.Modules.Accounting.Domain.Entities;

public sealed class AccountingBook : OrganizationEntity
{
    public AccountingBook(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public string Code { get; }
    public string Name { get; }
    public string Currency { get; }
}
