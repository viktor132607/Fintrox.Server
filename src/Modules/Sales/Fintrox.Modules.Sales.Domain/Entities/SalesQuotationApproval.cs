namespace Fintrox.Modules.Sales.Domain.Entities;

/// <summary>Organization-scoped immutable SalesQuotationApproval model; no cross-module navigations.</summary>
public sealed class SalesQuotationApproval : OrganizationEntity
{
    public SalesQuotationApproval(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid salesQuotationId,
        Guid approverUserId,
        string decision,
        DateTimeOffset decidedAtUtc)
        : base(id, organizationId, createdAtUtc)
    {
        SalesQuotationId = DomainRules.Id(salesQuotationId, nameof(salesQuotationId));
        ApproverUserId = DomainRules.Id(approverUserId, nameof(approverUserId));
        Decision = DomainRules.Text(decision, nameof(decision));
        DecidedAtUtc = DomainRules.Instant(decidedAtUtc, nameof(decidedAtUtc));
    }

    public Guid SalesQuotationId { get; }
    public Guid ApproverUserId { get; }
    public string Decision { get; }
    public DateTimeOffset DecidedAtUtc { get; }
}
