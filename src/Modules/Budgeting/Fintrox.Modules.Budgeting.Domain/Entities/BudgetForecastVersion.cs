namespace Fintrox.Modules.Budgeting.Domain.Entities;

/// <summary>Tenant-owned BudgetForecastVersion record. Domain invariants enforced at construction.</summary>
public sealed class BudgetForecastVersion : OrganizationEntity
{
    public BudgetForecastVersion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid forecastId,
        int versionNumber,
        Guid preparedByUserId,
        DateTimeOffset preparedAtUtc,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        ForecastId = DomainRules.Id(forecastId, nameof(forecastId));
        VersionNumber = DomainRules.PositiveInt(versionNumber, nameof(versionNumber));
        PreparedByUserId = DomainRules.Id(preparedByUserId, nameof(preparedByUserId));
        PreparedAtUtc = DomainRules.Instant(preparedAtUtc, nameof(preparedAtUtc));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid ForecastId { get; }
    public int VersionNumber { get; }
    public Guid PreparedByUserId { get; }
    public DateTimeOffset PreparedAtUtc { get; }
    public string Status { get; }
}
