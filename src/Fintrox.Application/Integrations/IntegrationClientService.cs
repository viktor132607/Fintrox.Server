using Fintrox.Application.Authorization;
using Fintrox.Application.Common.Interfaces;
using Fintrox.Application.Organizations;
using Fintrox.Contracts.Integrations;
using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public sealed class IntegrationClientService(
    IIntegrationClientRepository repository,
    IOrganizationRepository organizationRepository,
    IIntegrationAccessTokenService accessTokenService,
    ICurrentOrganization currentOrganization,
    TimeProvider timeProvider) : IIntegrationClientService
{
    public async Task<IReadOnlyList<IntegrationClientResponse>> ListAsync(
        bool includeInactive,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var clients = await repository.ListAsync(
            organizationId,
            includeInactive,
            cancellationToken);

        return clients.Select(Map).ToArray();
    }

    public async Task<IntegrationClientResponse?> GetAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var client = await repository.GetAsync(
            organizationId,
            integrationClientId,
            trackChanges: false,
            cancellationToken);

        return client is null ? null : Map(client);
    }

    public async Task<IntegrationClientSecretResponse> CreateAsync(
        CreateIntegrationClientRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        await EnsureActiveOrganizationAsync(
            organizationId,
            cancellationToken);

        var scopes = IntegrationScopes.Normalize(request.Scopes);
        var clientId = await GenerateUniqueClientIdAsync(cancellationToken);
        var secret = IntegrationCredentialUtility.GenerateSecret();
        var now = timeProvider.GetUtcNow();

        var client = IntegrationClient.Create(
            organizationId,
            clientId,
            request.Name,
            IntegrationCredentialUtility.HashSecret(secret),
            IntegrationCredentialUtility.GetSecretPrefix(secret),
            IntegrationScopes.Serialize(scopes),
            now);

        await repository.AddAsync(client, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return new IntegrationClientSecretResponse(
            Map(client),
            secret);
    }

    public async Task<IntegrationClientResponse?> UpdateAsync(
        Guid integrationClientId,
        UpdateIntegrationClientRequest request,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var client = await repository.GetAsync(
            organizationId,
            integrationClientId,
            trackChanges: true,
            cancellationToken);

        if (client is null)
        {
            return null;
        }

        client.Update(
            request.Name,
            IntegrationScopes.Serialize(request.Scopes),
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);
        return Map(client);
    }

    public async Task<IntegrationClientSecretResponse?> RotateSecretAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var client = await repository.GetAsync(
            organizationId,
            integrationClientId,
            trackChanges: true,
            cancellationToken);

        if (client is null)
        {
            return null;
        }

        var secret = IntegrationCredentialUtility.GenerateSecret();

        client.RotateSecret(
            IntegrationCredentialUtility.HashSecret(secret),
            IntegrationCredentialUtility.GetSecretPrefix(secret),
            timeProvider.GetUtcNow());

        await repository.SaveChangesAsync(cancellationToken);

        return new IntegrationClientSecretResponse(
            Map(client),
            secret);
    }

    public async Task<bool> DeactivateAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        var client = await repository.GetAsync(
            organizationId,
            integrationClientId,
            trackChanges: true,
            cancellationToken);

        if (client is null)
        {
            return false;
        }

        client.Deactivate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IntegrationClientResponse?> ActivateAsync(
        Guid integrationClientId,
        CancellationToken cancellationToken)
    {
        var organizationId = currentOrganization.RequireOrganizationId();
        await EnsureActiveOrganizationAsync(
            organizationId,
            cancellationToken);

        var client = await repository.GetAsync(
            organizationId,
            integrationClientId,
            trackChanges: true,
            cancellationToken);

        if (client is null)
        {
            return null;
        }

        client.Activate(timeProvider.GetUtcNow());
        await repository.SaveChangesAsync(cancellationToken);
        return Map(client);
    }

    public async Task<IntegrationTokenResponse> ExchangeTokenAsync(
        IntegrationTokenRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(
                request.GrantType?.Trim(),
                "client_credentials",
                StringComparison.Ordinal))
        {
            throw new IntegrationAuthenticationException(
                "Only the client_credentials grant type is supported.");
        }

        var normalizedClientId = request.ClientId?.Trim();

        if (string.IsNullOrWhiteSpace(normalizedClientId) ||
            string.IsNullOrWhiteSpace(request.ClientSecret))
        {
            throw InvalidCredentials();
        }

        var client = await repository.GetByClientIdAsync(
            normalizedClientId,
            trackChanges: true,
            cancellationToken);

        if (client is null ||
            !client.IsActive ||
            !IntegrationCredentialUtility.VerifySecret(
                request.ClientSecret,
                client.SecretHash))
        {
            throw InvalidCredentials();
        }

        await EnsureActiveOrganizationAsync(
            client.OrganizationId,
            cancellationToken);

        var scopes = IntegrationScopes.Parse(client.Scopes);
        var token = accessTokenService.Create(
            client.Id,
            client.ClientId,
            client.Name,
            client.OrganizationId,
            scopes);

        var now = timeProvider.GetUtcNow();
        client.MarkUsed(now);
        await repository.SaveChangesAsync(cancellationToken);

        return new IntegrationTokenResponse(
            token.Value,
            "Bearer",
            token.ExpiresAtUtc,
            Math.Max(
                0L,
                (long)(token.ExpiresAtUtc - now).TotalSeconds),
            string.Join(' ', scopes));
    }

    private async Task<string> GenerateUniqueClientIdAsync(
        CancellationToken cancellationToken)
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var clientId = IntegrationCredentialUtility.GenerateClientId();

            if (!await repository.ClientIdExistsAsync(
                    clientId,
                    cancellationToken))
            {
                return clientId;
            }
        }

        throw new IntegrationClientConflictException(
            "Could not allocate a unique integration client id.");
    }

    private async Task EnsureActiveOrganizationAsync(
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        var organization = await organizationRepository.GetAsync(
            organizationId,
            trackChanges: false,
            cancellationToken);

        if (organization is null || !organization.IsActive)
        {
            throw new IntegrationClientConflictException(
                "The organization does not exist or is inactive.");
        }
    }

    private static IntegrationAuthenticationException InvalidCredentials() =>
        new("Invalid integration client credentials.");

    private static IntegrationClientResponse Map(
        IntegrationClient client) =>
        new(
            client.Id,
            client.OrganizationId,
            client.ClientId,
            client.Name,
            IntegrationScopes.Parse(client.Scopes),
            client.SecretPrefix,
            client.IsActive,
            client.SecretRotatedAtUtc,
            client.LastUsedAtUtc,
            client.CreatedAtUtc,
            client.UpdatedAtUtc);
}
