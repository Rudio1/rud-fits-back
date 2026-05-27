using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RudFitAI.Domain.Entities;

namespace RudFitAI.Infrastructure.Persistence.Configurations;

public sealed class PaymentEventConfiguration : IEntityTypeConfiguration<PaymentEvent>
{
    public void Configure(EntityTypeBuilder<PaymentEvent> builder)
    {
        builder.ToTable("PaymentEvents");
        builder.HasKey(paymentEvent => paymentEvent.Id);

        builder.Property(paymentEvent => paymentEvent.ExternalEventId).HasMaxLength(200).IsRequired();
        builder.Property(paymentEvent => paymentEvent.EventType).HasMaxLength(100).IsRequired();
        builder.Property(paymentEvent => paymentEvent.PayloadJson).IsRequired();

        builder.HasIndex(paymentEvent => paymentEvent.ExternalEventId).IsUnique();
    }
}
