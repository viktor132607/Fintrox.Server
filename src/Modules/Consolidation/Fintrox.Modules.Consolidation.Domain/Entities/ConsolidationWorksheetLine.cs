namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationWorksheetLine; external references validated by application layer.</summary>
public sealed class ConsolidationWorksheetLine : OrganizationEntity
{
    public ConsolidationWorksheetLine(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid worksheetId,
        string groupAccountCode,
        decimal debit,
        decimal credit,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        WorksheetId = DomainRules.Id(worksheetId, nameof(worksheetId));
        GroupAccountCode = DomainRules.Text(groupAccountCode, nameof(groupAccountCode));
        Debit = DomainRules.Nonnegative(debit, nameof(debit));
        Credit = DomainRules.Nonnegative(credit, nameof(credit));
        Currency = DomainRules.Currency(currency, nameof(currency));
        if (Debit == 0m && Credit == 0m) throw new ArgumentException("Both debit and credit cannot be zero.");
    }

    public Guid WorksheetId { get; }
    public string GroupAccountCode { get; }
    public decimal Debit { get; }
    public decimal Credit { get; }
    public string Currency { get; }
}
