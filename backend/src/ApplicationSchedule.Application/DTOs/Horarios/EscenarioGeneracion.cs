namespace ApplicationSchedule.Application.DTOs.Horarios;

public static class EscenarioGeneracion
{
    /// <summary>
    /// DTO que representa un escenario de generación con sus parámetros (p. ej. jornada, perfiles).
    /// </summary>
    public const string IngDiurna = "ING_DIURNA";
    public const string IngNocturna = "ING_NOCTURNA";
    public const string TapsiDiurna = "TAPSI_DIURNA";
    public const string TapsiNocturna = "TAPSI_NOCTURNA";

    public static readonly HashSet<string> Todos = new(StringComparer.OrdinalIgnoreCase)
    {
        IngDiurna,
        IngNocturna,
        TapsiDiurna,
        TapsiNocturna
    };

    public static bool EsTapsi(string escenario)
    {
        return escenario.Equals(TapsiDiurna, StringComparison.OrdinalIgnoreCase)
            || escenario.Equals(TapsiNocturna, StringComparison.OrdinalIgnoreCase);
    }

    public static bool EsDiurno(string escenario)
    {
        return escenario.Equals(IngDiurna, StringComparison.OrdinalIgnoreCase)
            || escenario.Equals(TapsiDiurna, StringComparison.OrdinalIgnoreCase);
    }
}