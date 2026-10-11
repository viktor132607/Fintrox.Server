namespace Fintrox.Modules.Reporting.Domain.Entities;

public sealed class ReportDefinition : OrganizationEntity
{
    public ReportDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string name,
        string definitionJson)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        Name = DomainRules.Text(name, nameof(name));
        DefinitionJson = DomainRules.Json(definitionJson, nameof(definitionJson));
    }

    public string Code { get; }
    public string Name { get; }
    public string DefinitionJson { get; }
}
