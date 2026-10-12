namespace Fintrox.Modules.Accounting.Domain.Entities;

/// <summary>Organization-scoped AccountSegment domain record; referenced objects are validated by application use cases.</summary>
public sealed class AccountSegment : OrganizationEntity
{
    public AccountSegment(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        int length,
        bool isRequired)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Length = DomainRules.PositiveInt(length, nameof(length));
        IsRequired = isRequired;
    }

    public string Code { get; }
    public string Name { get; }
    public int Length { get; }
    public bool IsRequired { get; }
}
