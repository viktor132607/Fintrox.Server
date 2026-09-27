namespace Fintrox.Contracts.Payments;

public sealed record DocumentSettlementResponse(
    string TargetType,
    Guid TargetDocumentId,
    string DocumentNumber,
    string DocumentStatus,
    Guid CounterpartyId,
    Guid CurrencyId,
    string CurrencyCode,
    decimal DocumentTotal,
    decimal AllocatedAmount,
    decimal OutstandingAmount,
    string SettlementStatus);
