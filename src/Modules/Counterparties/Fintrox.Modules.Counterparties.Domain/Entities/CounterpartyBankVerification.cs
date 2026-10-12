namespace Fintrox.Modules.Counterparties.Domain.Entities;

/// <summary>Tenant-scoped CounterpartyBankVerification. All foreign references are IDs, not cross-module navigation properties.</summary>
public sealed class CounterpartyBankVerification : OrganizationEntity
{
    public CounterpartyBankVerification(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid counterpartyBankAccountId,
        string verificationMethod,
        DateTimeOffset verifiedAtUtc,
        Guid verifiedByUserId)
        : base(id, organizationId, createdAtUtc)
    {
        CounterpartyBankAccountId = DomainRules.Id(counterpartyBankAccountId, nameof(counterpartyBankAccountId));
        VerificationMethod = DomainRules.Text(verificationMethod, nameof(verificationMethod));
        VerifiedAtUtc = DomainRules.Instant(verifiedAtUtc, nameof(verifiedAtUtc));
        VerifiedByUserId = DomainRules.Id(verifiedByUserId, nameof(verifiedByUserId));
    }

    public Guid CounterpartyBankAccountId { get; }
    public string VerificationMethod { get; }
    public DateTimeOffset VerifiedAtUtc { get; }
    public Guid VerifiedByUserId { get; }
}
