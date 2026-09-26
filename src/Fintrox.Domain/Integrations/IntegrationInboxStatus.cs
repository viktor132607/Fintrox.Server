namespace Fintrox.Domain.Integrations;

public enum IntegrationInboxStatus
{
    Received = 0,
    Processing = 10,
    Succeeded = 20,
    Failed = 30
}
