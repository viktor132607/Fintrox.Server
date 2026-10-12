namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportColumn model. References require application validation.</summary>
public sealed class ReportColumn : OrganizationEntity
{
    public ReportColumn(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportLayoutId,
        string fieldCode,
        string header,
        int position,
        bool isVisible)
        : base(id, organizationId, createdAtUtc)
    {
        ReportLayoutId = DomainRules.Id(reportLayoutId, nameof(reportLayoutId));
        FieldCode = DomainRules.Text(fieldCode, nameof(fieldCode));
        Header = DomainRules.Text(header, nameof(header));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        IsVisible = isVisible;
    }

    public Guid ReportLayoutId { get; }
    public string FieldCode { get; }
    public string Header { get; }
    public int Position { get; }
    public bool IsVisible { get; }
}
