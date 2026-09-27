using Fintrox.Domain.Integrations;

namespace Fintrox.Application.Integrations;

public interface IIntegrationOutboxService
{
    Task<IntegrationEvent> EnqueueAsync(
        Guid organizationId,
        Guid? inboxId,
        IntegrationBusinessEventResult result,
        CancellationToken cancellationToken);
}
