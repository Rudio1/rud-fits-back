namespace RudFitAI.Application.Options;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";

    /// <summary>
    /// Fuso para gravar horários de parede no banco (auditoria e instante atual quando omitido).
    /// Ex.: America/Sao_Paulo (Linux/macOS) ou E. South America Standard Time (Windows).
    /// Vazio = DateTime.Now do servidor.
    /// </summary>
    public string BusinessTimeZoneId { get; init; } = "America/Sao_Paulo";
}
