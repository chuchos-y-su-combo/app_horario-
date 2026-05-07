namespace ApplicationSchedule.Domain.Entities;


public class Profesor
{
    public int IdProfesor { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    
    public string Identificacion { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipos permitidos: "Tiempo Completo" o "Parcial"
    /// </summary>
    public string TipoContrato { get; set; } = string.Empty;
}