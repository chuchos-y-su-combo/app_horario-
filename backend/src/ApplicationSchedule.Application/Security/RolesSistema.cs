namespace ApplicationSchedule.Application.Security;

public static class RolesSistema
{
    public const string Administrador = "Administrador";
    public const string Coordinador = "Coordinador";

    public const string AdministradorOCoordinador = Administrador + "," + Coordinador;
}