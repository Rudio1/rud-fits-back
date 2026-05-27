using System.Text.Json;
using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Subscriptions;

public sealed class AsaasWebhookProcessor : IAsaasWebhookProcessor
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly SubscriptionDomainService _subscriptionDomainService;

    public AsaasWebhookProcessor(
        ISubscriptionRepository subscriptionRepository,
        SubscriptionDomainService subscriptionDomainService)
    {
        _subscriptionRepository = subscriptionRepository;
        _subscriptionDomainService = subscriptionDomainService;
    }

    public async Task<bool> ProcessAsync(string payloadJson, CancellationToken cancellationToken)
    {
        using JsonDocument document = JsonDocument.Parse(payloadJson);
        JsonElement root = document.RootElement;

        string eventType = ReadString(root, "event") ?? "UNKNOWN";
        string externalEventId = BuildExternalEventId(root, eventType, payloadJson);

        if (await _subscriptionRepository.PaymentEventExistsAsync(externalEventId, cancellationToken))
        {
            return true;
        }

        PaymentEvent paymentEvent = new(
            Guid.NewGuid(),
            externalEventId,
            eventType,
            payloadJson,
            DateTime.UtcNow);

        await _subscriptionRepository.AddPaymentEventAsync(paymentEvent, cancellationToken);

        UserSubscription? subscription = await ResolveSubscriptionAsync(root, cancellationToken);
        if (subscription is not null)
        {
            ApplyEvent(subscription, eventType, root);
            await _subscriptionRepository.SaveChangesAsync(cancellationToken);
            return true;
        }

        await _subscriptionRepository.SaveChangesAsync(cancellationToken);
        return true;
    }

    private void ApplyEvent(UserSubscription subscription, string eventType, JsonElement root)
    {
        switch (eventType)
        {
            case "PAYMENT_RECEIVED":
                HandlePaymentReceived(subscription, root);
                break;
            case "PAYMENT_OVERDUE":
                if (subscription.Status == SubscriptionStatus.Active
                    || subscription.Status == SubscriptionStatus.Trialing)
                {
                    subscription.MarkPastDue();
                }

                break;
            case "PAYMENT_REFUNDED":
            case "PAYMENT_DELETED":
                subscription.Expire();
                break;
            case "PIX_AUTOMATIC_RECURRING_AUTHORIZATION_ACTIVATED":
                subscription.SetBillingType(BillingType.PixAutomatic);
                break;
            default:
                break;
        }
    }

    private void HandlePaymentReceived(UserSubscription subscription, JsonElement root)
    {
        _subscriptionDomainService.EnsureCanActivateFromPayment(subscription);

        SubscriptionPlan plan = subscription.SubscriptionPlan;
        DateTime periodStartUtc = DateTime.UtcNow;
        BillingType billingType = subscription.BillingType == BillingType.None
            ? BillingType.Pix
            : subscription.BillingType;

        if (plan.Kind == PlanKind.OneTime)
        {
            subscription.Activate(billingType, periodStartUtc, periodEndUtc: null);
            return;
        }

        DateTime periodEndUtc = _subscriptionDomainService.CalculateMonthlyPeriodEndUtc(periodStartUtc);
        subscription.Activate(billingType, periodStartUtc, periodEndUtc);
    }

    private async Task<UserSubscription?> ResolveSubscriptionAsync(
        JsonElement root,
        CancellationToken cancellationToken)
    {
        JsonElement payment = TryGetProperty(root, "payment");
        JsonElement subscriptionElement = TryGetProperty(root, "subscription");
        JsonElement authorization = TryGetProperty(root, "authorization");

        string? paymentId = ReadString(payment, "id");
        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            UserSubscription? byPayment =
                await _subscriptionRepository.GetUserSubscriptionByAsaasPaymentIdAsync(paymentId, cancellationToken);
            if (byPayment is not null)
            {
                return byPayment;
            }
        }

        string? subscriptionId = ReadString(subscriptionElement, "id") ?? ReadString(payment, "subscription");
        if (!string.IsNullOrWhiteSpace(subscriptionId))
        {
            UserSubscription? bySubscription =
                await _subscriptionRepository.GetUserSubscriptionByAsaasSubscriptionIdAsync(
                    subscriptionId,
                    cancellationToken);
            if (bySubscription is not null)
            {
                return bySubscription;
            }
        }

        string? customerId = ReadString(payment, "customer")
            ?? ReadString(subscriptionElement, "customer")
            ?? ReadString(authorization, "customerId");

        if (!string.IsNullOrWhiteSpace(customerId))
        {
            return await _subscriptionRepository.GetUserSubscriptionByAsaasCustomerIdAsync(
                customerId,
                cancellationToken);
        }

        return null;
    }

    private static string BuildExternalEventId(JsonElement root, string eventType, string payloadJson)
    {
        JsonElement payment = TryGetProperty(root, "payment");
        string? paymentId = ReadString(payment, "id");
        if (!string.IsNullOrWhiteSpace(paymentId))
        {
            return $"{eventType}:{paymentId}";
        }

        string? authorizationId = ReadString(TryGetProperty(root, "authorization"), "id");
        if (!string.IsNullOrWhiteSpace(authorizationId))
        {
            return $"{eventType}:{authorizationId}";
        }

        return $"{eventType}:{payloadJson.GetHashCode(StringComparison.Ordinal)}";
    }

    private static JsonElement TryGetProperty(JsonElement element, string propertyName)
    {
        if (element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out JsonElement value))
        {
            return value;
        }

        return default;
    }

    private static string? ReadString(JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        if (!element.TryGetProperty(propertyName, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }
}
