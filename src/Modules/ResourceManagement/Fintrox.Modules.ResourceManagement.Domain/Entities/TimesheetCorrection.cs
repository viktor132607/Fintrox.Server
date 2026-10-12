namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable TimesheetCorrection model; no cross-module navigations.</summary>
public sealed class TimesheetCorrection : OrganizationEntity
{
    public TimesheetCorrection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid timesheetEntryId,
        decimal correctedHours,
        string reason,
        Guid correctedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        TimesheetEntryId = DomainRules.Id(timesheetEntryId, nameof(timesheetEntryId));
        CorrectedHours = DomainRules.Nonnegative(correctedHours, nameof(correctedHours));
        Reason = DomainRules.Text(reason, nameof(reason));
        CorrectedByUserId = DomainRules.Id(correctedByUserId, nameof(correctedByUserId));
    }

    public Guid TimesheetEntryId { get; }
    public decimal CorrectedHours { get; }
    public string Reason { get; }
    public Guid CorrectedByUserId { get; }
}
