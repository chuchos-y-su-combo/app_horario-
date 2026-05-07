using System.ComponentModel.DataAnnotations;

namespace ApplicationSchedule.Application.DTOs.Profesores;

public class ActualizarProfesorRequest
{
    [Required(ErrorMessage = "El nombre del profesor es obligatorio.")]
    [MaxLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La identificación es obligatoria.")]
    [MaxLength(20, ErrorMessage = "La identificación no puede superar los 20 caracteres.")]
    public string Identificacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo de contrato es obligatorio.")]
    [RegularExpression(@"^(Tiempo Completo|Parcial)$", ErrorMessage = "El tipo de contrato debe ser 'Tiempo Completo' o 'Parcial'.")]
    public string TipoContrato { get; set; } = string.Empty;
}
