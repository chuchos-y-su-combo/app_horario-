namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class ConflictoAsignacionResponse
{
    /// <summary>
    /// Resultado general del análisis de conflictos para el semestre.
    /// </summary>
    public string Semestre { get; set; } = string.Empty;

    public int TotalConflictos { get; set; }

    public bool TieneConflictos => TotalConflictos > 0;

    public List<AlertaConflicto> Conflictos { get; set; } = new();
}

public class AlertaConflicto
{
    /// <summary>
    /// Tipo de conflicto detectado.
    /// CruceHorario | ExcesoCarga | AsignaturasSinDocente | DocenteSinHorario | FranjaBloqueadaAsignatura
    /// </summary>
    public string TipoConflicto { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de severidad: Error | Advertencia
    /// </summary>
    public string Severidad { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public string? IdDocente { get; set; }

    public string? NombreDocente { get; set; }

    public string? IdAsignacion1 { get; set; }

    public string? IdAsignacion2 { get; set; }

    public string? NombreAsignatura { get; set; }

    public string? DetalleHorario { get; set; }
}