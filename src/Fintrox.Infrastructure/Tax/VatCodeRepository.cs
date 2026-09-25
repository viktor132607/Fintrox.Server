using Fintrox.Application.Tax;
using Fintrox.Domain.Tax;
using Fintrox.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Fintrox.Infrastructure.Tax;

public sealed class VatCodeRepository(
    FintroxDbContext dbContext) : IVatCodeRepository
{
    public async Task<IReadOnlyList<VatCode>> ListAsync(
        Guid organizationId,
        bool includeInactive,
        DateOnly? asOfDate,
        bool? appliesToSales,
        bool? appliesToPurchases,
        CancellationToken cancellationToken)
    {
        IQueryable<VatCode> query = dbContext.VatCodes
            .AsNoTracking()
            .Where(vatCode => vatCode.OrganizationId == organizationId);

        if (!includeInactive)
        {
            query = query.Where(vatCode => vatCode.IsActive);
        }

        if (asOfDate.HasValue)
        {
            var date = asOfDate.Value;
            query = query.Where(vatCode =>
                vatCode.ValidFrom <= date &&
                (!vatCode.ValidTo.HasValue ||
                 vatCode.ValidTo.Value >= date));
        }

        if (appliesToSales.HasValue)
        {
            query = query.Where(vatCode =>
                vatCode.AppliesToSales == appliesToSales.Value);
        }

        if (appliesToPurchases.HasValue)
        {
            query = query.Where(vatCode =>
                vatCode.AppliesToPurchases == appliesToPurchases.Value);
        }

        return await query
            .OrderBy(vatCode => vatCode.Code)
            .ThenByDescending(vatCode => vatCode.ValidFrom)
            .ToArrayAsync(cancellationToken);
    }

    public async Task<VatCode?> GetAsync(
        Guid organizationId,
        Guid vatCodeId,
        bool trackChanges,
        CancellationToken cancellationToken)
    {
        IQueryable<VatCode> query = dbContext.VatCodes;

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.SingleOrDefaultAsync(
            vatCode =>
                vatCode.OrganizationId == organizationId &&
                vatCode.Id == vatCodeId,
            cancellationToken);
    }

    public Task<bool> VersionOverlapsAsync(
        Guid organizationId,
        string code,
        DateOnly validFrom,
        DateOnly? validTo,
        Guid? excludingVatCodeId,
        CancellationToken cancellationToken)
    {
        return dbContext.VatCodes.AnyAsync(
            vatCode =>
                vatCode.OrganizationId == organizationId &&
                vatCode.Code == code &&
                (!excludingVatCodeId.HasValue ||
                 vatCode.Id != excludingVatCodeId.Value) &&
                (!vatCode.ValidTo.HasValue ||
                 vatCode.ValidTo.Value >= validFrom) &&
                (!validTo.HasValue ||
                 vatCode.ValidFrom <= validTo.Value),
            cancellationToken);
    }

    public async Task AddAsync(
        VatCode vatCode,
        CancellationToken cancellationToken)
    {
        await dbContext.VatCodes.AddAsync(vatCode, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
