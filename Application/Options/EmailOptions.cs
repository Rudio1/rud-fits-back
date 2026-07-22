namespace RudFitAI.Application.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SmtpHost { get; init; } = string.Empty;

    public int SmtpPort { get; init; } = 587;

    public string SmtpUser { get; init; } = string.Empty;

    public string SmtpPassword { get; init; } = string.Empty;

    public bool EnableSsl { get; init; } = true;

    public string FromAddress { get; init; } = string.Empty;

    public string FromName { get; init; } = "RudFit";

    public string InviteBaseUrl { get; init; } = string.Empty;
}
