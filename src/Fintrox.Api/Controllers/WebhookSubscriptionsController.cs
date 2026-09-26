using Fintrox.Application.Authorization;
using Fintrox.Application.Integrations;
using Fintrox.Contracts.Integrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fintrox.Api.Controllers;

[ApiController]
[Authorize(Policy = Permissions.IntegrationsManage)]
[Route("api/v1/integrations/webhooks")]
public sealed class WebhookSubscriptionsController(
    IWebhookSubscriptionService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WebhookSubscriptionResponse>>> List(
        [FromQuery] bool includeInactive,
        CancellationToken cancellationToken) =>
        Ok(await service.ListAsync(
            includeInactive,
            cancellationToken));

    [HttpGet("{subscriptionId:guid}")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> Get(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription = await service.GetAsync(
            subscriptionId,
            cancellationToken);

        return subscription is null
            ? NotFound()
            : Ok(subscription);
    }

    [HttpPost]
    public async Task<ActionResult<WebhookSubscriptionSecretResponse>> Create(
        CreateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await service.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(Get),
                new
                {
                    subscriptionId = subscription.Subscription.Id
                },
                subscription);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                "Invalid webhook subscription",
                exception.Message));
        }
    }

    [HttpPut("{subscriptionId:guid}")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> Update(
        Guid subscriptionId,
        UpdateWebhookSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await service.UpdateAsync(
                subscriptionId,
                request,
                cancellationToken);

            return subscription is null
                ? NotFound()
                : Ok(subscription);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                "Invalid webhook subscription",
                exception.Message));
        }
    }

    [HttpPost("{subscriptionId:guid}/rotate-secret")]
    public async Task<ActionResult<WebhookSubscriptionSecretResponse>> RotateSecret(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription = await service.RotateSecretAsync(
            subscriptionId,
            cancellationToken);

        return subscription is null
            ? NotFound()
            : Ok(subscription);
    }

    [HttpDelete("{subscriptionId:guid}")]
    public async Task<IActionResult> Deactivate(
        Guid subscriptionId,
        CancellationToken cancellationToken) =>
        await service.DeactivateAsync(
            subscriptionId,
            cancellationToken)
            ? NoContent()
            : NotFound();

    [HttpPost("{subscriptionId:guid}/activate")]
    public async Task<ActionResult<WebhookSubscriptionResponse>> Activate(
        Guid subscriptionId,
        CancellationToken cancellationToken)
    {
        var subscription = await service.ActivateAsync(
            subscriptionId,
            cancellationToken);

        return subscription is null
            ? NotFound()
            : Ok(subscription);
    }

    [HttpGet("deliveries")]
    public async Task<ActionResult<IReadOnlyList<WebhookDeliveryResponse>>> Deliveries(
        [FromQuery] Guid? subscriptionId,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await service.ListDeliveriesAsync(
                subscriptionId,
                status,
                cancellationToken));
        }
        catch (ArgumentException exception)
        {
            return BadRequest(ToProblem(
                "Invalid webhook delivery query",
                exception.Message));
        }
    }

    private static ProblemDetails ToProblem(
        string title,
        string detail) =>
        new()
        {
            Status = StatusCodes.Status400BadRequest,
            Title = title,
            Detail = detail
        };
}
