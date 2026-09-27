using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Counterparties;
using Fintrox.Domain.Partners;

namespace Fintrox.Application.Counterparties;

public sealed class CounterpartyService(
    ICounterpartyRepository repository,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : ICounterpartyService
{
    public async Task<IReadOnlyList<CounterpartyResponse>> ListAsync(
        bool includeInactive,
        string? search,
        string? role,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var (isCustomer, isSupplier) = ParseRole(role);
        var normalizedSearch = string.IsNullOrWhiteSpace(search)
            ? null
            : search.Trim();

        var counterparties = await repository.ListAsync(
            organizationId,
            includeInactive,
            normalizedSearch,
            isCustomer,
            isSupplier,
            cancellationToken);

        return counterparties
            .OrderBy(counterparty => counterparty.Code, StringComparer.Ordinal)
            .Select(Map)
            .ToArray();
    }

    public async Task<CounterpartyResponse?> GetAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var counterparty = await repository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: false,
            cancellationToken);

        return counterparty is null ? null : Map(counterparty);
    }

    public async Task<CounterpartyResponse> CreateAsync(
        CreateCounterpartyRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRoleSelection(request.IsCustomer, request.IsSupplier);

        var organizationId = currentOrganization.RequireOrganizationId();
        var normalizedCode = NormalizeCode(request.Code);
        var countryCode = NormalizeCountryCode(request.CountryCode);
        var registrationNumber = NormalizeIdentifier(request.RegistrationNumber);
        var vatNumber = NormalizeIdentifier(request.VatNumber);

        await EnsureUniqueAsync(
            organizationId,
            normalizedCode,
            countryCode,
            registrationNumber,
            vatNumber,
            excludingCounterpartyId: null,
            cancellationToken);

        var counterparty = Counterparty.Create(
            organizationId,
            normalizedCode,
            request.Name,
            request.LegalName,
            countryCode,
            registrationNumber,
            vatNumber,
            request.IsCustomer,
            request.IsSupplier,
            request.PaymentTermDays,
            request.ContactPerson,
            request.Email,
            request.Phone,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.PostalCode,
            request.Website,
            request.Notes,
            timeProvider.GetUtcNow());

        await repository.AddAsync(counterparty, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Map(counterparty);
    }

    public async Task<CounterpartyResponse?> UpdateAsync(
        Guid counterpartyId,
        UpdateCounterpartyRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var counterparty = await repository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: true,
            cancellationToken);

        if (counterparty is null)
        {
            return null;
        }

        ValidateRoleSelection(request.IsCustomer, request.IsSupplier);

        var normalizedCode = NormalizeCode(request.Code);
        var countryCode = NormalizeCountryCode(request.CountryCode);
        var registrationNumber = NormalizeIdentifier(request.RegistrationNumber);
        var vatNumber = NormalizeIdentifier(request.VatNumber);

        await EnsureUniqueAsync(
            organizationId,
            normalizedCode,
            countryCode,
            registrationNumber,
            vatNumber,
            counterpartyId,
            cancellationToken);

        counterparty.Update(
            normalizedCode,
            request.Name,
            request.LegalName,
            countryCode,
            registrationNumber,
            vatNumber,
            request.IsCustomer,
            request.IsSupplier,
            request.PaymentTermDays,
            request.ContactPerson,
            request.Email,
            request.Phone,
            request.AddressLine1,
            request.AddressLine2,
            request.City,
            request.PostalCode,
            request.Website,
            request.Notes,
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);

        return Map(counterparty);
    }

    public async Task<bool> DeactivateAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var counterparty = await repository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: true,
            cancellationToken);

        if (counterparty is null)
        {
            return false;
        }

        counterparty.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<CounterpartyResponse?> ActivateAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var counterparty = await repository.GetAsync(
            organizationId,
            counterpartyId,
            trackChanges: true,
            cancellationToken);

        if (counterparty is null)
        {
            return null;
        }

        counterparty.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);

        return Map(counterparty);
    }

    private async Task EnsureUniqueAsync(
        Guid organizationId,
        string code,
        string countryCode,
        string? registrationNumber,
        string? vatNumber,
        Guid? excludingCounterpartyId,
        CancellationToken cancellationToken)
    {
        if (await repository.CodeExistsAsync(
                organizationId,
                code,
                excludingCounterpartyId,
                cancellationToken))
        {
            throw new CounterpartyConflictException(
                $"Counterparty code '{code}' already exists in this organization.");
        }

        if (registrationNumber is not null &&
            await repository.RegistrationNumberExistsAsync(
                organizationId,
                countryCode,
                registrationNumber,
                excludingCounterpartyId,
                cancellationToken))
        {
            throw new CounterpartyConflictException(
                $"Registration number '{registrationNumber}' already exists for country '{countryCode}'.");
        }

        if (vatNumber is not null &&
            await repository.VatNumberExistsAsync(
                organizationId,
                countryCode,
                vatNumber,
                excludingCounterpartyId,
                cancellationToken))
        {
            throw new CounterpartyConflictException(
                $"VAT number '{vatNumber}' already exists for country '{countryCode}'.");
        }
    }

    private static (bool? IsCustomer, bool? IsSupplier) ParseRole(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return (null, null);
        }

        return role.Trim().ToLowerInvariant() switch
        {
            "customer" => (true, null),
            "supplier" => (null, true),
            "both" => (true, true),
            _ => throw new CounterpartyQueryException(
                "Role must be 'customer', 'supplier' or 'both'.")
        };
    }

    private static string NormalizeCode(string value) =>
        value.Trim().ToUpperInvariant();

    private static string NormalizeCountryCode(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();

        if (normalized.Length != 2 ||
            !normalized.All(char.IsAsciiLetter))
        {
            throw new CounterpartyConflictException(
                "Country code must contain exactly two ASCII letters.");
        }

        return normalized;
    }

    private static void ValidateRoleSelection(
        bool isCustomer,
        bool isSupplier)
    {
        if (!isCustomer && !isSupplier)
        {
            throw new CounterpartyConflictException(
                "A counterparty must be a customer, a supplier, or both.");
        }
    }

    private static string? NormalizeIdentifier(string? value)
    {
        var normalized = value?.Trim();
        return string.IsNullOrWhiteSpace(normalized)
            ? null
            : normalized.ToUpperInvariant();
    }

    private static CounterpartyResponse Map(Counterparty counterparty)
    {
        return new CounterpartyResponse(
            counterparty.Id,
            counterparty.OrganizationId,
            counterparty.Code,
            counterparty.Name,
            counterparty.LegalName,
            counterparty.CountryCode,
            counterparty.RegistrationNumber,
            counterparty.VatNumber,
            counterparty.IsCustomer,
            counterparty.IsSupplier,
            counterparty.PaymentTermDays,
            counterparty.ContactPerson,
            counterparty.Email,
            counterparty.Phone,
            counterparty.AddressLine1,
            counterparty.AddressLine2,
            counterparty.City,
            counterparty.PostalCode,
            counterparty.Website,
            counterparty.Notes,
            counterparty.IsActive,
            counterparty.CreatedAtUtc,
            counterparty.CreatedByUserId,
            counterparty.UpdatedAtUtc,
            counterparty.UpdatedByUserId);
    }
}
