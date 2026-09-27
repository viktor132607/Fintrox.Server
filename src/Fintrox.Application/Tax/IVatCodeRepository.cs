using Fintrox.Domain.Tax;

namespace Fintrox.Application.Tax;

public interface IVatCodeRepository
{
    Task<IReadOnlyList<VatCode>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        DateOnly? asOfDate,
        bool? appliesToSales,
        bool? appliesToPurchases,
        CancellationToken cancellationToken);

    Task<VatCode?> GetAsync(
        Guid organizationId,
        Guid vatCodeId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> VersionOverlapsAsync(
        Guid organizationId,
        string code,
        DateOnly validFrom,
        DateOnly? validTo,
        Guid? excludingVatCodeId,
        CancellationToken cancellationToken);

    Task AddAsync(
        VatCode vatCode,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
