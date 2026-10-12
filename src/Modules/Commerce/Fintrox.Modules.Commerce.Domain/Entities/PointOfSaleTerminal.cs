namespace Fintrox.Modules.Commerce.Domain.Entities;

/// <summary>Tenant-owned PointOfSaleTerminal; external references validated by application layer.</summary>
public sealed class PointOfSaleTerminal : OrganizationEntity
{
    public PointOfSaleTerminal(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid storefrontId,
        string terminalCode,
        string locationCode,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        StorefrontId = DomainRules.Id(storefrontId, nameof(storefrontId));
        TerminalCode = DomainRules.Text(terminalCode, nameof(terminalCode));
        LocationCode = DomainRules.Text(locationCode, nameof(locationCode));
        IsActive = isActive;
    }

    public Guid StorefrontId { get; }
    public string TerminalCode { get; }
    public string LocationCode { get; }
    public bool IsActive { get; }
}
