namespace Fintrox.Modules.Projects.Domain.Entities;

/// <summary>Immutable tenant-scoped ProjectInvoiceMilestone; cross-module IDs are verified by use cases.</summary>
public sealed class ProjectInvoiceMilestone : OrganizationEntity
{
    public ProjectInvoiceMilestone(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid projectId,
        Guid milestoneId,
        Guid invoiceReferenceId,
        decimal amount,
        string currency)
        : base(id, organizationId, createdAtUtc)
    {
        ProjectId = DomainRules.Id(projectId, nameof(projectId));
        MilestoneId = DomainRules.Id(milestoneId, nameof(milestoneId));
        InvoiceReferenceId = DomainRules.Id(invoiceReferenceId, nameof(invoiceReferenceId));
        Amount = DomainRules.Nonnegative(amount, nameof(amount));
        Currency = DomainRules.Currency(currency, nameof(currency));
    }

    public Guid ProjectId { get; }
    public Guid MilestoneId { get; }
    public Guid InvoiceReferenceId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
}
