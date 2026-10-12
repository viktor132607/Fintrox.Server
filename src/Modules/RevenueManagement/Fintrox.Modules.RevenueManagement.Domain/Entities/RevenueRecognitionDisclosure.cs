namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

/// <summary>Organization-scoped immutable RevenueRecognitionDisclosure model; no cross-module navigations.</summary>
public sealed class RevenueRecognitionDisclosure : OrganizationEntity
{
    public RevenueRecognitionDisclosure(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid accountingPeriodId,
        string disclosureType,
        decimal amount,
        string currency,
        string comment)
        : base(id, organizationId, createdAtUtc)
    {
        AccountingPeriodId = DomainRules.Id(accountingPeriodId, nameof(accountingPeriodId));
        DisclosureType = DomainRules.Text(disclosureType, nameof(disclosureType));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        Comment = DomainRules.Text(comment, nameof(comment));
    }

    public Guid AccountingPeriodId { get; }
    public string DisclosureType { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string Comment { get; }
}
