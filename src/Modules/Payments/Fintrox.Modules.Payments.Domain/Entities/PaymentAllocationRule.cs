namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentAllocationRule; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentAllocationRule : OrganizationEntity
{
    public PaymentAllocationRule(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string code,
        string documentType,
        int allocationPriority,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        Code = DomainRules.Text(code, nameof(code));
        DocumentType = DomainRules.Text(documentType, nameof(documentType));
        AllocationPriority = DomainRules.NonnegativeInt(allocationPriority, nameof(allocationPriority));
        IsActive = isActive;
    }

    public string Code { get; }
    public string DocumentType { get; }
    public int AllocationPriority { get; }
    public bool IsActive { get; }
}
