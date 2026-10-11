namespace Fintrox.Modules.Payroll.Domain.Entities;

public sealed class PayrollComponent : OrganizationEntity
{
    public PayrollComponent(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        PayrollComponentKind kind)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Kind = DomainRules.Defined(kind, nameof(kind));
    }

    public string Code { get; }
    public string Name { get; }
    public PayrollComponentKind Kind { get; }
}
