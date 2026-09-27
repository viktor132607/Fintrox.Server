using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Fintrox.Application.Authorization;
using Fintrox.Application.Identity;
using Fintrox.Application.Integrations;
using Microsoft.IdentityModel.Tokens;

namespace Fintrox.Api.Authentication;

public sealed class JwtIntegrationAccessTokenService(
    JwtOptions options,
    TimeProvider timeProvider) : IIntegrationAccessTokenService
{
    public AccessToken Create(
        Guid integrationClientId,
        string clientId,
        string clientName,
        Guid organizationId,
        IReadOnlyCollection<string> scopes)
    {
        var now = timeProvider.GetUtcNow();
        var expiresAt = now.AddMinutes(
            options.IntegrationAccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                integrationClientId.ToString()),
            new(
                JwtRegisteredClaimNames.Name,
                clientName),
            new(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString()),
            new(
                IntegrationClaims.ActorType,
                IntegrationClaims.IntegrationActor),
            new(
                IntegrationClaims.ClientId,
                clientId),
            new(
                IntegrationClaims.OrganizationId,
                organizationId.ToString())
        };

        claims.AddRange(scopes.Select(
            scope => new Claim(
                IntegrationClaims.Scope,
                scope)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(options.SigningKey));

        var credentials = new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new AccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAt);
    }
}
