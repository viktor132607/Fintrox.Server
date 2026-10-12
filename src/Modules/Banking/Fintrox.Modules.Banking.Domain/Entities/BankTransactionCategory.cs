namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankTransactionCategory record; cross-module references require application authorization.</summary>
public sealed class BankTransactionCategory : OrganizationEntity
{
    public BankTransactionCategory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        Guid defaultAccountId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        DefaultAccountId = DomainRules.Id(defaultAccountId, nameof(defaultAccountId));
        IsActive = isActive;
    }

    public string Code { get; }
    public string Name { get; }
    public Guid DefaultAccountId { get; }
    public bool IsActive { get; }
}
