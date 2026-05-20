namespace ApplicationSchedule.Application.DTOs.Asignaciones;

/// <summary>
/// DTO devuelto tras confirmar asignaciones, con resultados por asignación.
/// </summary>
public class ResultadoConfirmacionResponse
{
    public int Confirmadas { get; set; }

    public int Fallidas { get; set; }

    public List<string> IdsConfirmadas { get; set; } = new();

    public List<ResultadoFallidoItem> Errores { get; set; } = new();
}

public class ResultadoFallidoItem
{
    /// <summary>
    /// Identificador de la asignación que no pudo confirmarse.
    /// </summary>
    public string IdAsignacion { get; set; } = string.Empty;

    /// <summary>
    /// Motivo del fallo al confirmar la asignación.
    /// </summary>
    public string Motivo { get; set; } = string.Empty;
}