namespace RudFitAI.Application.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public string ApiKey { get; init; } = string.Empty;

    public string Model { get; init; } = "gpt-4o-mini";

    public int RequestTimeoutSeconds { get; init; } = 10;

    public int MaxCompletionTokens { get; init; } = 512;

    public string ChatCompletionsUrl { get; init; } = "https://api.openai.com/v1/chat/completions";
}
