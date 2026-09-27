using Fintrox.Application.Common.Interfaces;
using Fintrox.Contracts.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class IntegrationRequestService(
    IIntegrationRequestRepository repository,
    ICurrentOrganization currentOrganization) : IIntegrationRequestService
{
    public async Task<IReadOnlyList<IntegrationRequestResponse>> ListAsync(
        string? sourceSystem,
        string? externalId,
        string? eventType,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var requests = await repository.ListAsync(
            organizationId,
            NormalizeOptional(sourceSystem, 100, toLower: true),
            NormalizeOptional(externalId, 200, toLower: false),
            NormalizeOptional(eventType, 100, toLower: true),
            cancellationToken);

        return requests.Select(Map).ToArray();
    }

    public async Task<IntegrationRequestResponse?> GetAsync(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var request = await repository.GetAsync(
            organizationId,
            requestId,
            cancellationToken);

        return request is null
            ? null
            : Map(request);
    }

    private static IntegrationRequestResponse Map(
        IntegrationRequestRecord request) =>
        new(
            request.Id,
            request.OrganizationId,
            request.IntegrationClientId,
            request.SourceSystem,
            request.ExternalId,
            request.EventType,
            request.RequestMethod,
            request.RequestPath,
            request.RequestHash,
            request.Status.ToString(),
            request.ResponseStatusCode,
            request.ResourceReference,
            request.CreatedAtUtc,
            request.CompletedAtUtc);

    private static string? NormalizeOptional(
        string? value,
        int maxLength,
        bool toLower)
    {
        var normalized = value?.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        if (normalized.Length > maxLength)
        {
            throw new ArgumentException(
                $"Value cannot exceed {maxLength} characters.",
                nameof(value));
        }

        return toLower
            ? normalized.ToLowerInvariant()
            : normalized;
    }
}
