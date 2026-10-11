namespace Fintrox.Modules.Accounting.Domain.Entities;

public sealed class AccountingDimensionValue : OrganizationEntity
{
    public AccountingDimensionValue(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid dimensionId,
        string code,
        string name)
        : base(id, organizationId, createdAtUtc)
    {
        DimensionId = DomainRules.Id(dimensionId, nameof(dimensionId));
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
    }

    public Guid DimensionId { get; }
    public string Code { get; }
    public string Name { get; }
}
