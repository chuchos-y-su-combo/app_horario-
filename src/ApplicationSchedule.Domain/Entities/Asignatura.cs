namespace ApplicationSchedule.Domain.Entities;

public class Asignatura
{
	public int IdAsignatura { get; set; }

	public int IdPlanEstudios { get; set; }

	public string Codigo { get; set; } = string.Empty;

	public string Nombre { get; set; } = string.Empty;

	public int Creditos { get; set; }

	public int Semestre { get; set; }

	public bool EsFijaTapsi { get; set; }
}