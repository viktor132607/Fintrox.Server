namespace Fintrox.Modules.WarehouseManagement.Domain.Entities;

/// <summary>Immutable, module-owned CarrierRateQuote; tenant scope and input fields validated at creation.</summary>
public sealed class CarrierRateQuote : OrganizationEntity
{
    public CarrierRateQuote(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid shipmentId,
        string carrierCode,
        string serviceCode,
        decimal quotedAmount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ShipmentId = DomainRules.Id(shipmentId, nameof(shipmentId));
        CarrierCode = DomainRules.Text(carrierCode, nameof(carrierCode));
        ServiceCode = DomainRules.Text(serviceCode, nameof(serviceCode));
        QuotedAmount = DomainRules.Nonnegative(quotedAmount, nameof(quotedAmount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ShipmentId { get; }
    public string CarrierCode { get; }
    public string ServiceCode { get; }
    public decimal QuotedAmount { get; }
    public string Currency { get; }
}
