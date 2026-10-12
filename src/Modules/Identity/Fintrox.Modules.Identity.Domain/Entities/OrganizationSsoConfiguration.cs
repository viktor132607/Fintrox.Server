namespace Fintrox.Modules.Identity.Domain.Entities;

/// <summary>Immutable tenant-owned OrganizationSsoConfiguration domain record; references checked in application layer.</summary>
public sealed class OrganizationSsoConfiguration : OrganizationEntity
{
    public OrganizationSsoConfiguration(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string providerCode,
        string issuerUri,
        string clientIdentifier,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ProviderCode = DomainRules.Text(providerCode, nameof(providerCode));
        IssuerUri = DomainRules.Text(issuerUri, nameof(issuerUri));
        ClientIdentifier = DomainRules.Text(clientIdentifier, nameof(clientIdentifier));
        IsEnabled = isEnabled;
    }

    public string ProviderCode { get; }
    public string IssuerUri { get; }
    public string ClientIdentifier { get; }
    public bool IsEnabled { get; }
}
