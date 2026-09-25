using Fintrox.Contracts.Counterparties;

namespace Fintrox.Application.Counterparties;

public interface ICounterpartyService
{
    Task<IReadOnlyList<CounterpartyResponse>> ListAsync(
        bool includeInactive,
        string? search,
        string? role,
        CancellationToken cancellationToken);

    Task<CounterpartyResponse?> GetAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken);

    Task<CounterpartyResponse> CreateAsync(
        CreateCounterpartyRequest request,
        CancellationToken cancellationToken);

    Task<CounterpartyResponse?> UpdateAsync(
        Guid counterpartyId,
        UpdateCounterpartyRequest request,
        CancellationToken cancellationToken);

    Task<bool> DeactivateAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken);

    Task<CounterpartyResponse?> ActivateAsync(
        Guid counterpartyId,
        CancellationToken cancellationToken);
}
