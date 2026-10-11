namespace Fintrox.Modules.Purchases.Domain.Entities;

public sealed class PurchaseRequisition : OrganizationEntity
{
    public PurchaseRequisition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string reference,
        Guid requestedByUserId,
        DateOnly requiredOn)
        : base(id, organizationId, createdAtUtc)
    {
        Reference = DomainRules.Text(reference, nameof(reference));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        RequiredOn = DomainRules.Date(requiredOn, nameof(requiredOn));
    }

    public string Reference { get; }
    public Guid RequestedByUserId { get; }
    public DateOnly RequiredOn { get; }
}
