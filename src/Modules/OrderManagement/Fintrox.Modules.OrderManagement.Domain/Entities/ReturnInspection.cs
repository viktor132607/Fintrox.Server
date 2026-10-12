namespace Fintrox.Modules.OrderManagement.Domain.Entities;

/// <summary>Immutable, organization-owned ReturnInspection record.</summary>
public sealed class ReturnInspection : OrganizationEntity
{
    public ReturnInspection(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid returnAuthorizationId,
        Guid inspectedByUserId,
        DateOnly inspectedOn,
        string result)
        : base(id, organizationId, createdAtUtc)
    {
        ReturnAuthorizationId = DomainRules.Id(returnAuthorizationId, nameof(returnAuthorizationId));
        InspectedByUserId = DomainRules.Id(inspectedByUserId, nameof(inspectedByUserId));
        InspectedOn = DomainRules.Date(inspectedOn, nameof(inspectedOn));
        Result = DomainRules.Text(result, nameof(result));
    }

    public Guid ReturnAuthorizationId { get; }
    public Guid InspectedByUserId { get; }
    public DateOnly InspectedOn { get; }
    public string Result { get; }
}
