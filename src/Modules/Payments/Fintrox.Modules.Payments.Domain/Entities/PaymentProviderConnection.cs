namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentProviderConnection; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentProviderConnection : OrganizationEntity
{
    public PaymentProviderConnection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string providerCode,
        string externalMerchantId,
        string credentialReference,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        ExternalMerchantId = DomainRules.Text(externalMerchantId, nameof(externalMerchantId));
        CredentialReference = DomainRules.Text(credentialReference, nameof(credentialReference));
        IsEnabled = isEnabled;
    }

    public string ProviderCode { get; }
    public string ExternalMerchantId { get; }
    public string CredentialReference { get; }
    public bool IsEnabled { get; }
}
