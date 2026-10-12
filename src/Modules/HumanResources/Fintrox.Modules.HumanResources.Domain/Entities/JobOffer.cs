namespace Fintrox.Modules.HumanResources.Domain.Entities;

/// <summary>Immutable organization-owned JobOffer domain model.</summary>
public sealed class JobOffer : OrganizationEntity
{
    public JobOffer(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid jobApplicationId,
        DateOnly offeredOn,
        decimal baseSalary,
        string currency,
        DateOnly expiresOn)
        : base(id, organizationId, createdAtUtc)
    {
        JobApplicationId = DomainRules.Id(jobApplicationId, nameof(jobApplicationId));
        OfferedOn = DomainRules.Date(offeredOn, nameof(offeredOn));
        BaseSalary = DomainRules.Nonnegative(baseSalary, nameof(baseSalary));
        Currency = DomainRules.Currency(currency, nameof(currency));
        ExpiresOn = DomainRules.Date(expiresOn, nameof(expiresOn));
    }

    public Guid JobApplicationId { get; }
    public DateOnly OfferedOn { get; }
    public decimal BaseSalary { get; }
    public string Currency { get; }
    public DateOnly ExpiresOn { get; }
}
