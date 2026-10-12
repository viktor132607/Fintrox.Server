namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyCommunicationPreference. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyCommunicationPreference : OrganizationEntity
{
    public CounterpartyCommunicationPreference(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyId,
        string channel,
        string languageCode,
        bool isOptedIn)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyId = DomainRules.Id(counterpartyId, nameof(counterpartyId));
        Channel = DomainRules.Text(channel, nameof(channel));
        LanguageCode = DomainRules.Text(languageCode, nameof(languageCode));
        IsOptedIn = isOptedIn;
    }

    public Guid CounterpartyId { get; }
    public string Channel { get; }
    public string LanguageCode { get; }
    public bool IsOptedIn { get; }
}
