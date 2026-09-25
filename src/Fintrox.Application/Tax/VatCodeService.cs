using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Tax;
using Fintrox.Domain.Tax;

namespace Fintrox.Application.Tax;

public sealed class VatCodeService(
    IVatCodeRepository repository,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IVatCodeService
{
    public async Task<IReadOnlyList<VatCodeResponse>> ListAsync(
        bool includeInactive,
        DateOnly? asOfDate,
        string? appliesTo,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var (sales, purchases) = ParseApplicability(appliesTo);

        var vatCodes = await repository.ListAsync(
            organizationId,
            includeInactive,
            asOfDate,
            sales,
            purchases,
            cancellationToken);

        return vatCodes
            .OrderBy(v => v.Code, StringComparer.Ordinal)
            .ThenByDescending(v => v.ValidFrom)
            .Select(Map)
            .ToArray();
    }

    public async Task<VatCodeResponse?> GetAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var vatCode = await repository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: false,
            cancellationToken);

        return vatCode is null ? null : Map(vatCode);
    }

    public async Task<VatCodeResponse> CreateAsync(
        CreateVatCodeRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var kind = ParseKind(request.Kind);
        var code = request.Code.Trim().ToUpperInvariant();

        await EnsureNoOverlapAsync(
            organizationId,
            code,
            request.ValidFrom,
            request.ValidTo,
            excludingVatCodeId: null,
            cancellationToken);

        var vatCode = VatCode.Create(
            organizationId,
            code,
            request.Name,
            kind,
            request.RatePercent,
            request.ValidFrom,
            request.ValidTo,
            request.AppliesToSales,
            request.AppliesToPurchases,
            timeProvider.GetUtcNow());

        await repository.AddAsync(vatCode, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(vatCode);
    }

    public async Task<VatCodeResponse?> UpdateAsync(
        Guid vatCodeId,
        UpdateVatCodeRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var vatCode = await repository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: true,
            cancellationToken);

        if (vatCode is null)
        {
            return null;
        }

        var kind = ParseKind(request.Kind);
        var code = request.Code.Trim().ToUpperInvariant();

        await EnsureNoOverlapAsync(
            organizationId,
            code,
            request.ValidFrom,
            request.ValidTo,
            vatCodeId,
            cancellationToken);

        vatCode.Update(
            code,
            request.Name,
            kind,
            request.RatePercent,
            request.ValidFrom,
            request.ValidTo,
            request.AppliesToSales,
            request.AppliesToPurchases,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return Map(vatCode);
    }

    public async Task<bool> DeactivateAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var vatCode = await repository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: true,
            cancellationToken);

        if (vatCode is null)
        {
            return false;
        }

        vatCode.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<VatCodeResponse?> ActivateAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var vatCode = await repository.GetAsync(
            organizationId,
            vatCodeId,
            trackChanges: true,
            cancellationToken);

        if (vatCode is null)
        {
            return null;
        }

        vatCode.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return Map(vatCode);
    }

    private async Task EnsureNoOverlapAsync(
        Guid organizationId,
        string code,
        DateOnly validFrom,
        DateOnly? validTo,
        Guid? excludingVatCodeId,
        CancellationToken cancellationToken)
    {
        if (await repository.VersionOverlapsAsync(
                organizationId,
                code,
                validFrom,
                validTo,
                excludingVatCodeId,
                cancellationToken))
        {
            throw new VatCodeConflictException(
                $"VAT code '{code}' already has a version overlapping the requested validity period.");
        }
    }

    private static VatCodeKind ParseKind(string value)
    {
        if (!Enum.TryParse<VatCodeKind>(
                value,
                ignoreCase: true,
                out var kind) ||
            !Enum.IsDefined(kind))
        {
            throw new VatCodeQueryException(
                $"Unknown VAT code kind '{value}'.");
        }

        return kind;
    }

    private static (bool? Sales, bool? Purchases) ParseApplicability(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return (null, null);
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "sales" => (true, null),
            "purchases" => (null, true),
            "both" => (true, true),
            _ => throw new VatCodeQueryException(
                "appliesTo must be 'sales', 'purchases' or 'both'.")
        };
    }

    private static VatCodeResponse Map(VatCode vatCode) =>
        new(
            vatCode.Id,
            vatCode.OrganizationId,
            vatCode.Code,
            vatCode.Name,
            vatCode.Kind.ToString(),
            vatCode.RatePercent,
            vatCode.ValidFrom,
            vatCode.ValidTo,
            vatCode.AppliesToSales,
            vatCode.AppliesToPurchases,
            vatCode.IsActive,
            vatCode.CreatedAtUtc,
            vatCode.CreatedByUserId,
            vatCode.UpdatedAtUtc,
            vatCode.UpdatedByUserId);
}
