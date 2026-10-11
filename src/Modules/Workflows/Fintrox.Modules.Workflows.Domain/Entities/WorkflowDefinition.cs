namespace Fintrox.Modules.Workflows.Domain.Entities;

public sealed class WorkflowDefinition : OrganizationEntity
{
    public WorkflowDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        int versionNumber)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
    }

    public string Code { get; }
    public string Name { get; }
    public int VersionNumber { get; }
}
