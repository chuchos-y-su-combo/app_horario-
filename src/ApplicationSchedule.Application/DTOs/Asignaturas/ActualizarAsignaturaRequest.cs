using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Asignaturas;

public class ActualizarAsignaturaRequest
{
    [Required(ErrorMessage = "El plan de estudios es obligatorio.")]
    public int IdPlanEstudios { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [MaxLength(20, ErrorMessage = "El código no puede superar los 20 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Los créditos son obligatorios.")]
    [Range(1, 20, ErrorMessage = "Los créditos deben estar entre 1 y 20.")]
    public int Creditos { get; set; }

    [Required(ErrorMessage = "El semestre es obligatorio.")]
    [Range(1, 10, ErrorMessage = "El semestre debe estar entre 1 y 10.")]
    public int Semestre { get; set; }

    public bool EsFijaTapsi { get; set; }
}
