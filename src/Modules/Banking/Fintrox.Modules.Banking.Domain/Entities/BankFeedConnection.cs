namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankFeedConnection record; cross-module references require application authorization.</summary>
public sealed class BankFeedConnection : OrganizationEntity
{
    public BankFeedConnection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        string providerName,
        string externalConnectionId,
        DateTimeOffset consentExpiresAtUtc,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        ProviderName = DomainRules.Text(providerName, nameof(providerName));
        ExternalConnectionId = DomainRules.Text(externalConnectionId, nameof(externalConnectionId));
        ConsentExpiresAtUtc = DomainRules.Instant(consentExpiresAtUtc, nameof(consentExpiresAtUtc));
        IsActive = isActive;
    }

    public Guid BankAccountId { get; }
    public string ProviderName { get; }
    public string ExternalConnectionId { get; }
    public DateTimeOffset ConsentExpiresAtUtc { get; }
    public bool IsActive { get; }
}
