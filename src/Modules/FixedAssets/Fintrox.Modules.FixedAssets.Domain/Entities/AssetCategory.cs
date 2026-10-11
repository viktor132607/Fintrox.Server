namespace Fintrox.Modules.FixedAssets.Domain.Entities;

public sealed class AssetCategory : OrganizationEntity
{
    public AssetCategory(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        int usefulLifeMonths)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        UsefulLifeMonths = DomainRules.PositiveInt(usefulLifeMonths, nameof(usefulLifeMonths));
    }

    public string Code { get; }
    public string Name { get; }
    public int UsefulLifeMonths { get; }
}
