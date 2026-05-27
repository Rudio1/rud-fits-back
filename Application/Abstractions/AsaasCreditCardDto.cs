namespace RudFitAI.Application.Abstractions;

public sealed class AsaasCreditCardDto
{
    public string HolderName { get; init; } = string.Empty;

    public string Number { get; init; } = string.Empty;

    public string ExpiryMonth { get; init; } = string.Empty;

    public string ExpiryYear { get; init; } = string.Empty;

    public string Ccv { get; init; } = string.Empty;
}
