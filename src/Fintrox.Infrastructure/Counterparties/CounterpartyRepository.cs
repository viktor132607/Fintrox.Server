using Fintrox.Application.Counterparties;
using Fintrox.Domain.Partners;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Counterparties;

public sealed class CounterpartyRepository(
    FintroxDbContext dbContext) : ICounterpartyRepository
{
    public async Task<IReadOnlyList<Counterparty>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        string? search,
        bool? isCustomer,
        bool? isSupplier,
        CancellationToken cancellationToken)
    {
        IQueryable<Counterparty> query = dbContext.Counterparties
            .AsNoTracking()
            .Where(counterparty =>
                counterparty.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(counterparty => counterparty.IsActive);
        }

        if (isCustomer.HasValue)
        {
            query = query.Where(counterparty =>
                counterparty.IsCustomer == isCustomer.Value);
        }

        if (isSupplier.HasValue)
        {
            query = query.Where(counterparty =>
                counterparty.IsSupplier == isSupplier.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim()}%";

            query = query.Where(counterparty =>
                EF.Functions.ILike(counterparty.Code, pattern) ||
                EF.Functions.ILike(counterparty.Name, pattern) ||
                (counterparty.LegalName != null &&
                 EF.Functions.ILike(counterparty.LegalName, pattern)) ||
                (counterparty.RegistrationNumber != null &&
                 EF.Functions.ILike(
                     counterparty.RegistrationNumber,
                     pattern)) ||
                (counterparty.VatNumber != null &&
                 EF.Functions.ILike(counterparty.VatNumber, pattern)) ||
                (counterparty.Email != null &&
                 EF.Functions.ILike(counterparty.Email, pattern)));
        }

        return await query
            .OrderBy(counterparty => counterparty.Code)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<Counterparty?> GetAsync(
        Guid organizationId,
        Guid counterpartyId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<Counterparty> query = dbContext.Counterparties;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            counterparty =>
                counterparty.OrganizationId == organizationId &&
                counterparty.Id == counterpartyId,
            cancellationToken);
    }

    public Task<bool> CodeExistsAsync(
        Guid organizationId,
        string code,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken)
    {
        return dbContext.Counterparties.AnyAsync(
            counterparty =>
                counterparty.OrganizationId == organizationId &&
                counterparty.Code == code &&
                (!excludingCounterpartyId.HasValue ||
                 counterparty.Id != excludingCounterpartyId.Value),
            cancellationToken);
    }

    public Task<bool> RegistrationNumberExistsAsync(
        Guid organizationId,
        string countryCode,
        string registrationNumber,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken)
    {
        return dbContext.Counterparties.AnyAsync(
            counterparty =>
                counterparty.OrganizationId == organizationId &&
                counterparty.CountryCode == countryCode &&
                counterparty.RegistrationNumber == registrationNumber &&
                (!excludingCounterpartyId.HasValue ||
                 counterparty.Id != excludingCounterpartyId.Value),
            cancellationToken);
    }

    public Task<bool> VatNumberExistsAsync(
        Guid organizationId,
        string countryCode,
        string vatNumber,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken)
    {
        return dbContext.Counterparties.AnyAsync(
            counterparty =>
                counterparty.OrganizationId == organizationId &&
                counterparty.CountryCode == countryCode &&
                counterparty.VatNumber == vatNumber &&
                (!excludingCounterpartyId.HasValue ||
                 counterparty.Id != excludingCounterpartyId.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        Counterparty counterparty,
        CancellationToken cancellationToken)
    {
        await dbContext.Counterparties.AddAsync(
            counterparty,
            cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
