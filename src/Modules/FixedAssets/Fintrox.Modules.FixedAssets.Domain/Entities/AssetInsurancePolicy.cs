namespace Fintrox.Modules.FixedAssets.Domain.Entities;

/// <summary>Immutable organization-owned AssetInsurancePolicy domain model.</summary>
public sealed class AssetInsurancePolicy : OrganizationEntity
{
    public AssetInsurancePolicy(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid fixedAssetId,
        string policyNumber,
        string providerName,
        DateOnly startsOn,
        DateOnly endsOn)
        : base(id, organizationId, createdAtUtc)
    {
        FixedAssetId = DomainRules.Id(fixedAssetId, nameof(fixedAssetId));
        PolicyNumber = DomainRules.Text(policyNumber, nameof(policyNumber));
        ProviderName = DomainRules.Text(providerName, nameof(providerName));
        StartsOn = DomainRules.Date(startsOn, nameof(startsOn));
        EndsOn = DomainRules.Date(endsOn, nameof(endsOn));
        if (EndsOn < StartsOn) throw new ArgumentException("End must not precede start.", nameof(endsOn));
    }

    public Guid FixedAssetId { get; }
    public string PolicyNumber { get; }
    public string ProviderName { get; }
    public DateOnly StartsOn { get; }
    public DateOnly EndsOn { get; }
}
