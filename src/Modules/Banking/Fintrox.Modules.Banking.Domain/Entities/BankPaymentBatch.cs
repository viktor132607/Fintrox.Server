namespace Fintrox.Modules.Banking.Domain.Entities;

/// <summary>Tenant-scoped BankPaymentBatch record; cross-module references require application authorization.</summary>
public sealed class BankPaymentBatch : OrganizationEntity
{
    public BankPaymentBatch(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        string batchReference,
        DateOnly requestedOn,
        Guid requestedByUserId,
        string status)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        BatchReference = DomainRules.Text(batchReference, nameof(batchReference));
        RequestedOn = DomainRules.Date(requestedOn, nameof(requestedOn));
        RequestedByUserId = DomainRules.Id(requestedByUserId, nameof(requestedByUserId));
        Status = DomainRules.Text(status, nameof(status));
    }

    public Guid BankAccountId { get; }
    public string BatchReference { get; }
    public DateOnly RequestedOn { get; }
    public Guid RequestedByUserId { get; }
    public string Status { get; }
}
