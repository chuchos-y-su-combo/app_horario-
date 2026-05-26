namespace ApplicationSchedule.Application.DTOs.Asignaciones;

/// <summary>
/// DTO que representa un conflicto detectado entre asignaciones (solapamientos, disponibilidad, etc.).
/// </summary>
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

    /// <summary>
    /// Descripción humana del conflicto detectado.
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del docente relacionado, cuando aplique.
    /// </summary>
    public string? IdDocente { get; set; }

    /// <summary>
    /// Nombre del docente relacionado, cuando aplique.
    /// </summary>
    public string? NombreDocente { get; set; }

    /// <summary>
    /// Identificador de la primera asignación involucrada, cuando aplique.
    /// </summary>
    public string? IdAsignacion1 { get; set; }

    /// <summary>
    /// Identificador de la segunda asignación involucrada, cuando aplique.
    /// </summary>
    public string? IdAsignacion2 { get; set; }

    /// <summary>
    /// Nombre de la asignatura relacionada.
    /// </summary>
    public string? NombreAsignatura { get; set; }

    /// <summary>
    /// Detalle legible del horario asociado al conflicto.
    /// </summary>
    public string? DetalleHorario { get; set; }
}