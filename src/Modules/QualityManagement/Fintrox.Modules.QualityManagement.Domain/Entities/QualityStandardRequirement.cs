namespace Fintrox.Modules.QualityManagement.Domain.Entities;

/// <summary>Immutable, organization-scoped QualityStandardRequirement model. References require application validation.</summary>
public sealed class QualityStandardRequirement : OrganizationEntity
{
    public QualityStandardRequirement(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid standardId,
        string requirementCode,
        string description,
        bool isMandatory)
        : base(id, organizationId, createdAtUtc)
    {
        StandardId = DomainRules.Id(standardId, nameof(standardId));
        RequirementCode = DomainRules.Text(requirementCode, nameof(requirementCode));
        Description = DomainRules.Text(description, nameof(description));
        IsMandatory = isMandatory;
    }

    public Guid StandardId { get; }
    public string RequirementCode { get; }
    public string Description { get; }
    public bool IsMandatory { get; }
}
