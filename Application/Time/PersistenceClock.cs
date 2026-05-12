using RudFitAI.Application.Options;

namespace RudFitAI.Application.Time;

public static class PersistenceClock
{
    public static DateTime GetWallClockNow(PersistenceOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.BusinessTimeZoneId))
        {
            return DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
        }

        try
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(options.BusinessTimeZoneId.Trim());
            DateTime converted = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return DateTime.SpecifyKind(converted, DateTimeKind.Unspecified);
        }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidOperationException(
                $"Configuração Persistence:BusinessTimeZoneId inválida: '{options.BusinessTimeZoneId}'.",
                ex);
        }
    }
}
