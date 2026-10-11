namespace Fintrox.Modules.RevenueManagement.Domain.Entities;

public sealed class RevenueRecognitionRule : OrganizationEntity
{
    public RevenueRecognitionRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        RecognitionMethod method)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        Method = DomainRules.Defined(method, nameof(method));
    }

    public string Code { get; }
    public string Name { get; }
    public RecognitionMethod Method { get; }
}
