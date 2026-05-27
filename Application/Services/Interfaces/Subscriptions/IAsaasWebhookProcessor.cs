namespace RudFitAI.Application.Services.Interfaces.Subscriptions;

public interface IAsaasWebhookProcessor
{
    Task<bool> ProcessAsync(string payloadJson, CancellationToken cancellationToken);
}
