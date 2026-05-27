using RudFitAI.Domain.Common;

namespace RudFitAI.Domain.Entities;

public sealed class PaymentEvent : BaseEntity
{
    private PaymentEvent()
    {
    }

    public PaymentEvent(Guid id, string externalEventId, string eventType, string payloadJson, DateTime processedAtUtc)
        : this()
    {
        Id = id;
        ExternalEventId = externalEventId;
        EventType = eventType;
        PayloadJson = payloadJson;
        ProcessedAt = processedAtUtc;
    }

    public string ExternalEventId { get; private set; } = string.Empty;

    public string EventType { get; private set; } = string.Empty;

    public string PayloadJson { get; private set; } = string.Empty;

    public DateTime ProcessedAt { get; private set; }
}
