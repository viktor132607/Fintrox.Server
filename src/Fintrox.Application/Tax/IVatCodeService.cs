using Fintrox.Contracts.Tax;

namespace Fintrox.Application.Tax;

public interface IVatCodeService
{
    Task<IReadOnlyList<VatCodeResponse>> ListAsync(
        bool includeInactive,
        DateOnly? asOfDate,
        string? appliesTo,
        CancellationToken cancellationToken);

    Task<VatCodeResponse?> GetAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken);

    Task<VatCodeResponse> CreateAsync(
        CreateVatCodeRequest request,
        CancellationToken cancellationToken);

    Task<VatCodeResponse?> UpdateAsync(
        Guid vatCodeId,
        UpdateVatCodeRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken);

    Task<VatCodeResponse?> ActivateAsync(
        Guid vatCodeId,
        CancellationToken cancellationToken);
}
