namespace Fintrox.Modules.QualityManagement.Domain.Entities;

public sealed class QualityHold : OrganizationEntity
{
    public QualityHold(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid itemId,
        Guid lotId,
        string reason)
        : base(id, organizationId, createdAtUtc)
    {
        ItemId = DomainRules.Id(itemId, nameof(itemId));
        LotId = DomainRules.Id(lotId, nameof(lotId));
        Reason = DomainRules.Text(reason, nameof(reason));
    }

    public Guid ItemId { get; }
    public Guid LotId { get; }
    public string Reason { get; }
}
