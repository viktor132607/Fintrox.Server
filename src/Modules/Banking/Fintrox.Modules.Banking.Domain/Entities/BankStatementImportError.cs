namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankStatementImportError record; cross-module references require application authorization.</summary>
public sealed class BankStatementImportError : OrganizationEntity
{
    public BankStatementImportError(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid importId,
        int sourceLineNumber,
        string errorCode,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        ImportId = DomainRules.Id(importId, nameof(importId));
        SourceLineNumber = DomainRules.PositiveInt(sourceLineNumber, nameof(sourceLineNumber));
        ErrorCode = DomainRules.Text(errorCode, nameof(errorCode));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid ImportId { get; }
    public int SourceLineNumber { get; }
    public string ErrorCode { get; }
    public string Description { get; }
}
