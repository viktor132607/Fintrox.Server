using Fintrox.Domain.Partners;

namespace Fintrox.Application.Counterparties;

public interface ICounterpartyRepository
{
    Task<IReadOnlyList<Counterparty>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        string? search,
        bool? isCustomer,
        bool? isSupplier,
        CancellationToken cancellationToken);

    Task<Counterparty?> GetAsync(
        Guid organizationId,
        Guid counterpartyId,
        bool trackChanges,
        CancellationToken cancellationToken);

    Task<bool> CodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken);

    Task<bool> RegistrationNumberExistsAsync(
        Guid organizationId,
        string countryCode,
        string registrationNumber,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken);

    Task<bool> VatNumberExistsAsync(
        Guid organizationId,
        string countryCode,
        string vatNumber,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken);

    Task AddAsync(
        Counterparty counterparty,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
