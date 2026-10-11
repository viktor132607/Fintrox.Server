namespace Fintrox.Modules.FixedAssets.Domain.Entities;

public sealed class FixedAsset : OrganizationEntity
{
    public FixedAsset(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid categoryId,
        string code,
        string name,
        DateOnly acquiredOn,
        decimal acquisitionCost,
        decimal residualValue,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        CategoryId = DomainRules.Id(categoryId, nameof(categoryId));
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        AcquiredOn = DomainRules.Date(acquiredOn, nameof(acquiredOn));
        AcquisitionCost = DomainRules.Nonnegative(acquisitionCost, nameof(acquisitionCost));
        ResidualValue = DomainRules.Nonnegative(residualValue, nameof(residualValue));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (ResidualValue > AcquisitionCost) throw new ArgumentException("Residual value cannot exceed acquisition cost.", nameof(residualValue));
    }

    public Guid CategoryId { get; }
    public string Code { get; }
    public string Name { get; }
    public DateOnly AcquiredOn { get; }
    public decimal AcquisitionCost { get; }
    public decimal ResidualValue { get; }
    public string Currency { get; }
}
