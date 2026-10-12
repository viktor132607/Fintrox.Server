namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped CustomerSatisfactionSurvey domain record. No cross-module navigation properties.</summary>
public sealed class CustomerSatisfactionSurvey : OrganizationEntity
{
    public CustomerSatisfactionSurvey(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid supportTicketId,
        Guid customerId,
        decimal ratingPercent,
        DateOnly submittedOn,
        string comment)
        : base(id, organizationId, createdAtUtc)
    {
        SupportTicketId = DomainRules.Id(supportTicketId, nameof(supportTicketId));
        CustomerId = DomainRules.Id(customerId, nameof(customerId));
        RatingPercent = DomainRules.Percent(ratingPercent, nameof(ratingPercent));
        SubmittedOn = DomainRules.Date(submittedOn, nameof(submittedOn));
        Comment = DomainRules.Text(comment, nameof(comment));
    }

    public Guid SupportTicketId { get; }
    public Guid CustomerId { get; }
    public decimal RatingPercent { get; }
    public DateOnly SubmittedOn { get; }
    public string Comment { get; }
}
