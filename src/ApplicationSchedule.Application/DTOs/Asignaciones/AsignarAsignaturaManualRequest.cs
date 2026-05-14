using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class AsignarAsignaturaManualRequest
{
    [Required(ErrorMessage = "El docente es obligatorio.")]
    public string IdDocente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La asignatura es obligatoria.")]
    public string IdAsignatura { get; set; } = string.Empty;

    [Required(ErrorMessage = "El periodo es obligatorio.")]
    [MaxLength(10, ErrorMessage = "El periodo no puede superar los 10 caracteres.")]
    public string Periodo { get; set; } = string.Empty;

    /// <summary>
    /// Si es true, omite la validación de currículo. Usar solo en casos excepcionales.
    /// </summary>
    public bool ForzarSinCurriculo { get; set; } = false;
}