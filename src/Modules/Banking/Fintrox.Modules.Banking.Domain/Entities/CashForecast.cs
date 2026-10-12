namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped CashForecast record; cross-module references require application authorization.</summary>
public sealed class CashForecast : OrganizationEntity
{
    public CashForecast(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        DateOnly forecastOn,
        decimal expectedInflow,
        decimal expectedOutflow,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        ForecastOn = DomainRules.Date(forecastOn, nameof(forecastOn));
        ExpectedInflow = DomainRules.Nonnegative(expectedInflow, nameof(expectedInflow));
        ExpectedOutflow = DomainRules.Nonnegative(expectedOutflow, nameof(expectedOutflow));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid BankAccountId { get; }
    public DateOnly ForecastOn { get; }
    public decimal ExpectedInflow { get; }
    public decimal ExpectedOutflow { get; }
    public string Currency { get; }
}
