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
    private readonly ILogger<AsaasWebhooksController> _logger;

    public AsaasWebhooksController(
        IAsaasWebhookProcessor webhookProcessor,
        IOptions<AsaasOptions> asaasOptions,
        ILogger<AsaasWebhooksController> logger)
    {
        _webhookProcessor = webhookProcessor;
        _asaasOptions = asaasOptions.Value;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Receive(CancellationToken cancellationToken)
    {
        if (!IsWebhookAuthorized())
        {
            _logger.LogWarning(
                "Webhook Asaas rejeitado: token inválido ou ausente (header asaas-access-token).");
            return Unauthorized();
        }

        using StreamReader reader = new(Request.Body);
        string payloadJson = await reader.ReadToEndAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            _logger.LogWarning("Webhook Asaas recebido com corpo vazio.");
            return BadRequest(new { message = "Payload vazio." });
        }

        _logger.LogInformation(
            "Webhook Asaas HTTP recebido. ContentLength={ContentLength} Payload={Payload}",
            payloadJson.Length,
            payloadJson);

        await _webhookProcessor.ProcessAsync(payloadJson, cancellationToken);

        _logger.LogInformation("Webhook Asaas processado com sucesso (HTTP 200).");
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
