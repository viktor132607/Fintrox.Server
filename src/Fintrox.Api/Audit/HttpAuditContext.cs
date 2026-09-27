using Fintrox.Application.Common.Interfaces;

namespace Fintrox.Api.Audit;

public sealed class HttpAuditContext(
    ICurrentUser currentUser,
    ICurrentOrganization currentOrganization,
    IHttpContextAccessor httpContextAccessor) : IAuditContext
{
    public Guid? UserId => currentUser.UserId;

    public Guid? OrganizationId => currentOrganization.OrganizationId;

    public string? CorrelationId =>
        httpContextAccessor.HttpContext?.TraceIdentifier;
}
