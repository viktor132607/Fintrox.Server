using Fintrox.Application.Counterparties;

namespace Fintrox.Application.Integrations;

public sealed class CounterpartyIntegrationBusinessEventHandler(
    ICounterpartyService counterparties) : IIntegrationBusinessEventHandler
{
    public string Operation => IntegrationBusinessOperations.Counterparty;

    public async Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var payload = IntegrationPayloadSerializer
            .Deserialize<IntegrationCounterpartyPayload>(payloadJson);

        var counterparty = await counterparties.CreateAsync(
            payload.Counterparty,
            cancellationToken);

        return new IntegrationBusinessEventResult(
            "Counterparty",
            counterparty.Id,
            JournalEntryId: null,
            "counterparty.created",
            IntegrationPayloadSerializer.Serialize(counterparty));
    }
}
