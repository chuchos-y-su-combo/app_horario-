namespace ApplicationSchedule.Application.DTOs.Asignaciones;

public class ResultadoConfirmacionResponse
{
    public int Confirmadas { get; set; }

    public int Fallidas { get; set; }

    public List<string> IdsConfirmadas { get; set; } = new();

    public List<ResultadoFallidoItem> Errores { get; set; } = new();
}

public class ResultadoFallidoItem
{
    public string IdAsignacion { get; set; } = string.Empty;

    public string Motivo { get; set; } = string.Empty;
}