namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

public sealed class ServiceLevelAgreement : OrganizationEntity
{
    public ServiceLevelAgreement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string name,
        int responseMinutes,
        int resolutionMinutes)
        : base(id, organizationId, createdAtUtc)
    {
        Name = DomainRules.Text(name, nameof(name));
        ResponseMinutes = DomainRules.PositiveInt(responseMinutes, nameof(responseMinutes));
        ResolutionMinutes = DomainRules.PositiveInt(resolutionMinutes, nameof(resolutionMinutes));
        if (ResolutionMinutes < ResponseMinutes) throw new ArgumentException("Resolution must not precede the response target.", nameof(resolutionMinutes));
    }

    public string Name { get; }
    public int ResponseMinutes { get; }
    public int ResolutionMinutes { get; }
}
