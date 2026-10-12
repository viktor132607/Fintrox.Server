namespace Fintrox.Modules.Organizations.Domain.Entities;

/// <summary>Immutable, organization-owned OrganizationBankAccountLink record.</summary>
public sealed class OrganizationBankAccountLink : OrganizationEntity
{
    public OrganizationBankAccountLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid bankAccountId,
        Guid legalEntityId,
        bool isDefault,
        DateOnly linkedOn)
        : base(id, organizationId, createdAtUtc)
    {
        BankAccountId = DomainRules.Id(bankAccountId, nameof(bankAccountId));
        LegalEntityId = DomainRules.Id(legalEntityId, nameof(legalEntityId));
        IsDefault = isDefault;
        LinkedOn = DomainRules.Date(linkedOn, nameof(linkedOn));
    }

    public Guid BankAccountId { get; }
    public Guid LegalEntityId { get; }
    public bool IsDefault { get; }
    public DateOnly LinkedOn { get; }
}
