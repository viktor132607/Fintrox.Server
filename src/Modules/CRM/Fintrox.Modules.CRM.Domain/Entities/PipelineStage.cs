namespace Fintrox.Modules.CRM.Domain.Entities;

public sealed class PipelineStage : OrganizationEntity
{
    public PipelineStage(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid pipelineId,
        string name,
        int position,
        decimal probabilityPercent)
        : base(id, organizationId, createdAtUtc)
    {
        PipelineId = DomainRules.Id(pipelineId, nameof(pipelineId));
        Name = DomainRules.Text(name, nameof(name));
        Position = DomainRules.NonnegativeInt(position, nameof(position));
        ProbabilityPercent = DomainRules.Percent(probabilityPercent, nameof(probabilityPercent));
    }

    public Guid PipelineId { get; }
    public string Name { get; }
    public int Position { get; }
    public decimal ProbabilityPercent { get; }
}
