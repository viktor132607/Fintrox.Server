namespace Fintrox.Modules.ServiceManagement.Domain.Entities;

/// <summary>Tenant-scoped ServiceContractLink domain record. No cross-module navigation properties.</summary>
public sealed class ServiceContractLink : OrganizationEntity
{
    public ServiceContractLink(
        Guid id, Guid organizationId, DateTimeOffset createdAtUtc,
        Guid serviceLevelAgreementId,
        Guid contractId,
        DateOnly effectiveFrom,
        bool isActive)
        : base(id, organizationId, createdAtUtc)
    {
        ServiceLevelAgreementId = DomainRules.Id(serviceLevelAgreementId, nameof(serviceLevelAgreementId));
        ContractId = DomainRules.Id(contractId, nameof(contractId));
        EffectiveFrom = DomainRules.Date(effectiveFrom, nameof(effectiveFrom));
        IsActive = isActive;
    }

    public Guid ServiceLevelAgreementId { get; }
    public Guid ContractId { get; }
    public DateOnly EffectiveFrom { get; }
    public bool IsActive { get; }
}
