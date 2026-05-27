using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Subscriptions;

namespace RudFitAI.Web.Controllers;

[ApiController]
[Route("api/webhooks/asaas")]
public sealed class AsaasWebhooksController : ControllerBase
{
    private readonly IAsaasWebhookProcessor _webhookProcessor;
    private readonly AsaasOptions _asaasOptions;

    public AsaasWebhooksController(
        IAsaasWebhookProcessor webhookProcessor,
        IOptions<AsaasOptions> asaasOptions)
    {
        _webhookProcessor = webhookProcessor;
        _asaasOptions = asaasOptions.Value;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        if (!IsWebhookAuthorized())
        {
            return Unauthorized();
        }

        using StreamReader reader = new(Request.Body);
        string payloadJson = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return BadRequest(new { message = "Payload vazio." });
        }

        await _webhookProcessor.ProcessAsync(payloadJson, cancellationToken);
        return Ok();
    }

    private bool IsWebhookAuthorized()
    {
        if (string.IsNullOrWhiteSpace(_asaasOptions.WebhookAccessToken))
        {
            return true;
        }

        string? headerToken = Request.Headers["asaas-access-token"].FirstOrDefault();
        return string.Equals(headerToken, _asaasOptions.WebhookAccessToken, StringComparison.Ordinal);
    }
}
