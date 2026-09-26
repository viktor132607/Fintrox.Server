using Fintrox.Application.Accounting;
using Fintrox.Application.Payments;
using Fintrox.Contracts.Payments;

namespace Fintrox.Application.Integrations;

public sealed class PaymentIntegrationBusinessEventProcessor(
    IPaymentService payments,
    IJournalRepository journals) : IIntegrationBusinessEventProcessor
{
    public string Operation => IntegrationBusinessOperations.Payment;

    public async Task<IntegrationBusinessEventResult> HandleAsync(
        string payloadJson,
        CancellationToken cancellationToken)
    {
        var payload = IntegrationPayloadSerializer
            .Deserialize<IntegrationPaymentPayload>(payloadJson);

        var created = await payments.CreateAsync(
            new CreatePaymentRequest(
                payload.Direction,
                payload.CounterpartyId,
                payload.PaymentDate,
                payload.Method,
                payload.CurrencyId,
                payload.Amount,
                payload.Reference,
                payload.Notes),
            cancellationToken);

        foreach (var allocation in payload.Allocations ?? [])
        {
            var added = await payments.AddAllocationAsync(
                created.Id,
                allocation,
                cancellationToken);

            if (added is null)
            {
                throw new IntegrationProcessingException(
                    "A payment allocation could not be generated.");
            }
        }

        var confirmed = await payments.ConfirmAsync(
            created.Id,
            cancellationToken)
            ?? throw new IntegrationProcessingException(
                "The generated payment could not be confirmed.");

        var journal = await journals.GetSystemEntryByExternalReferenceAsync(
            confirmed.OrganizationId,
            $"payment:{confirmed.Id:N}",
            trackChanges: false,
            cancellationToken);

        return new IntegrationBusinessEventResult(
            "Payment",
            confirmed.Id,
            journal?.Id,
            "payment.confirmed",
            IntegrationPayloadSerializer.Serialize(confirmed));
    }
}
