namespace Fintrox.Modules.Consolidation.Domain.Entities;

/// <summary>Tenant-owned ConsolidationDisclosure; external references validated by application layer.</summary>
public sealed class ConsolidationDisclosure : OrganizationEntity
{
    public ConsolidationDisclosure(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid consolidationRunId,
        string disclosureCode,
        string title,
        string content)
        : base(id, organizationId, createdAtUtc)
    {
        ConsolidationRunId = DomainRules.Id(consolidationRunId, nameof(consolidationRunId));
        DisclosureCode = DomainRules.Text(disclosureCode, nameof(disclosureCode));
        Title = DomainRules.Text(title, nameof(title));
        Content = DomainRules.Text(content, nameof(content));
    }

    public Guid ConsolidationRunId { get; }
    public string DisclosureCode { get; }
    public string Title { get; }
    public string Content { get; }
}
