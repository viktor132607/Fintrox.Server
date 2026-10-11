namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class Nonconformity : OrganizationEntity
{
    public Nonconformity(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionId,
        string description)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionId = DomainRules.Id(inspectionId, nameof(inspectionId));
        Description = DomainRules.Text(description, nameof(description));
    }

    public Guid InspectionId { get; }
    public string Description { get; }
}
