namespace Fintrox.Modules.Capabilities.Domain.Entities;

public sealed class CapabilityDefinition : OrganizationEntity
{
    public CapabilityDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string moduleCode)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        ModuleCode = DomainRules.Text(moduleCode, nameof(moduleCode));
    }

    public string Code { get; }
    public string Name { get; }
    public string ModuleCode { get; }
}
