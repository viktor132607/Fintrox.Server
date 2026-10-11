namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class InspectionCriterion : OrganizationEntity
{
    public InspectionCriterion(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid inspectionPlanId,
        string name,
        decimal minimum,
        decimal maximum)
        : base(id, organizationId, createdAtUtc)
    {
        InspectionPlanId = DomainRules.Id(inspectionPlanId, nameof(inspectionPlanId));
        Name = DomainRules.Text(name, nameof(name));
        Minimum = minimum;
        Maximum = maximum;
        if (Maximum < Minimum) throw new ArgumentException("Maximum cannot be below minimum.", nameof(maximum));
    }

    public Guid InspectionPlanId { get; }
    public string Name { get; }
    public decimal Minimum { get; }
    public decimal Maximum { get; }
}
