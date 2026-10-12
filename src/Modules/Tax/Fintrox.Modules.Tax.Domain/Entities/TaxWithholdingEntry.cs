namespace Fintrox.Modules.Tax.Domain.Entities;

/// <summary>Immutable, module-owned TaxWithholdingEntry; tenant scope and input fields validated at creation.</summary>
public sealed class TaxWithholdingEntry : OrganizationEntity
{
    public TaxWithholdingEntry(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid sourcePaymentId,
        Guid withholdingRuleId,
        decimal withheldAmount,
        string currency,
        DateOnly withheldOn)
        : base(id, organizationId, createdAtUtc)
    {
        SourcePaymentId = DomainRules.Id(sourcePaymentId, nameof(sourcePaymentId));
        WithholdingRuleId = DomainRules.Id(withholdingRuleId, nameof(withholdingRuleId));
        WithheldAmount = DomainRules.Nonnegative(withheldAmount, nameof(withheldAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
        WithheldOn = DomainRules.Date(withheldOn, nameof(withheldOn));
    }

    public Guid SourcePaymentId { get; }
    public Guid WithholdingRuleId { get; }
    public decimal WithheldAmount { get; }
    public string Currency { get; }
    public DateOnly WithheldOn { get; }
}
