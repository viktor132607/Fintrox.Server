namespace Fintrox.Modules.Counterparties.Domain.Entities;

public sealed class PaymentTerm : OrganizationEntity
{
    public PaymentTerm(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        int dueDays)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        DueDays = DomainRules.NonnegativeInt(dueDays, nameof(dueDays));
    }

    public string Code { get; }
    public string Name { get; }
    public int DueDays { get; }
}
