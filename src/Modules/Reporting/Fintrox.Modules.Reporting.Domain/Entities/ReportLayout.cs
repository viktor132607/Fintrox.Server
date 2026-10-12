namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportLayout model. References require application validation.</summary>
public sealed class ReportLayout : OrganizationEntity
{
    public ReportLayout(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        string layoutType,
        string layoutJson,
        int versionNumber)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        LayoutType = DomainRules.Text(layoutType, nameof(layoutType));
        LayoutJson = DomainRules.Json(layoutJson, nameof(layoutJson));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
    }

    public Guid ReportDefinitionId { get; }
    public string LayoutType { get; }
    public string LayoutJson { get; }
    public int VersionNumber { get; }
}
