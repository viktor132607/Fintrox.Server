namespace Fintrox.Modules.ResourceManagement.Domain.Entities;

/// <summary>Organization-scoped immutable ResourceShiftTemplate model; no cross-module navigations.</summary>
public sealed class ResourceShiftTemplate : OrganizationEntity
{
    public ResourceShiftTemplate(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid resourceId,
        string shiftCode,
        string startTime,
        string endTime,
        decimal workingHours)
        : base(id, organizationId, createdAtUtc)
    {
        ResourceId = DomainRules.Id(resourceId, nameof(resourceId));
        ShiftCode = DomainRules.Text(shiftCode, nameof(shiftCode));
        StartTime = DomainRules.Text(startTime, nameof(startTime));
        EndTime = DomainRules.Text(endTime, nameof(endTime));
        WorkingHours = DomainRules.Positive(workingHours, nameof(workingHours));
    }

    public Guid ResourceId { get; }
    public string ShiftCode { get; }
    public string StartTime { get; }
    public string EndTime { get; }
    public decimal WorkingHours { get; }
}
