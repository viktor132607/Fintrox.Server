namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped DocumentNumberSequence domain record; referenced objects are validated by application use cases.</summary>
public sealed class DocumentNumberSequence : OrganizationEntity
{
    public DocumentNumberSequence(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string documentType,
        string prefix,
        int nextNumber,
        Guid fiscalYearId,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        Prefix = DomainRules.Text(prefix, nameof(prefix));
        NextNumber = DomainRules.PositiveInt(nextNumber, nameof(nextNumber));
        FiscalYearId = DomainRules.Id(fiscalYearId, nameof(fiscalYearId));
        IsActive = isActive;
    }

    public string DocumentType { get; }
    public string Prefix { get; }
    public int NextNumber { get; }
    public Guid FiscalYearId { get; }
    public bool IsActive { get; }
}
