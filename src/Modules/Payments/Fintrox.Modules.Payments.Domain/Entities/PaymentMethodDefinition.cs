namespace Fintrox.Modules.Payments.Domain.Entities;

/// <summary>Immutable tenant-scoped PaymentMethodDefinition; cross-module IDs are verified by use cases.</summary>
public sealed class PaymentMethodDefinition : OrganizationEntity
{
    public PaymentMethodDefinition(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        string methodCode,
        string name,
        string paymentChannel,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        MethodCode = DomainRules.Text(methodCode, nameof(methodCode));
        Name = DomainRules.Text(name, nameof(name));
        PaymentChannel = DomainRules.Text(paymentChannel, nameof(paymentChannel));
        IsActive = isActive;
    }

    public string MethodCode { get; }
    public string Name { get; }
    public string PaymentChannel { get; }
    public bool IsActive { get; }
}
