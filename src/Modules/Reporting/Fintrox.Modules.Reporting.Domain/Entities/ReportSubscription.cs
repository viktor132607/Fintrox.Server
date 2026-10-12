namespace Fintrox.Modules.Reporting.Domain.Entities;

/// <summary>Immutable, organization-scoped ReportSubscription model. References require application validation.</summary>
public sealed class ReportSubscription : OrganizationEntity
{
    public ReportSubscription(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid reportDefinitionId,
        Guid recipientUserId,
        string deliveryChannel,
        bool isEnabled)
        : base(id, organizationId, createdAtUtc)
    {
        ReportDefinitionId = DomainRules.Id(reportDefinitionId, nameof(reportDefinitionId));
        RecipientUserId = DomainRules.Id(recipientUserId, nameof(recipientUserId));
        DeliveryChannel = DomainRules.Text(deliveryChannel, nameof(deliveryChannel));
        IsEnabled = isEnabled;
    }

    public Guid ReportDefinitionId { get; }
    public Guid RecipientUserId { get; }
    public string DeliveryChannel { get; }
    public bool IsEnabled { get; }
}
