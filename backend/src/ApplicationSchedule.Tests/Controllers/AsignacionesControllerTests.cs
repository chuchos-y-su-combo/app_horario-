using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Pruebas de integración para el controlador de asignaciones.
/// </summary>
public class AsignacionesControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    private const string PlanDiurno = "11111111-1111-1111-1111-111111111111";

    public AsignacionesControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "POST /api/asignaciones - Crea asignación válida")]
    public async Task Crear_AsignacionValida()
    {
        var docente = await CrearDocenteAsync("TC");
        var asignatura = await CrearAsignaturaAsync("ASIG001");

        var request = new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = asignatura.IdAsignatura,
            Dia = 1,
            HoraInicio = "08:00",
            HoraFin = "10:00",
            Periodo = "2026-1"
        };

        var response = await Client.PostAsJsonAsync("/api/asignaciones", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignacion = await response.Content.ReadFromJsonAsync<AsignacionResponse>();
        asignacion.Should().NotBeNull();
        asignacion!.IdDocente.Should().Be(docente.IdProfesor);
        asignacion.IdAsignatura.Should().Be(asignatura.IdAsignatura);
        asignacion.AsignaturasActuales.Should().Be(1);
        asignacion.MaxAsignaturas.Should().Be(5);
        asignacion.Estado.Should().Be("Propuesta");
    }

    [Fact(DisplayName = "REQ 5 - Docente TP permite máximo 3 asignaturas distintas")]
    public async Task Req5_DocenteTP_RechazaCuartaAsignatura()
    {
        var docente = await CrearDocenteAsync("TP");

        var a1 = await CrearAsignaturaAsync("TP001");
        var a2 = await CrearAsignaturaAsync("TP002");
        var a3 = await CrearAsignaturaAsync("TP003");
        var a4 = await CrearAsignaturaAsync("TP004");

        await CrearAsignacionAsync(docente.IdProfesor, a1.IdAsignatura, 1, "08:00", "10:00");
        await CrearAsignacionAsync(docente.IdProfesor, a2.IdAsignatura, 2, "08:00", "10:00");
        await CrearAsignacionAsync(docente.IdProfesor, a3.IdAsignatura, 3, "08:00", "10:00");

        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = a4.IdAsignatura,
            Dia = 4,
            HoraInicio = "08:00",
            HoraFin = "10:00",
            Periodo = "2026-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("límite de 3");
    }

    [Fact(DisplayName = "REQ 5 - Docente TC permite máximo 5 asignaturas distintas")]
    public async Task Req5_DocenteTC_RechazaSextaAsignatura()
    {
        var docente = await CrearDocenteAsync("TC");

        var asignaturas = new List<AsignaturaResponse>();

        for (int i = 1; i <= 6; i++)
        {
            asignaturas.Add(await CrearAsignaturaAsync($"TC00{i}"));
        }

        for (int i = 0; i < 5; i++)
        {
            await CrearAsignacionAsync(
                docente.IdProfesor,
                asignaturas[i].IdAsignatura,
                i + 1,
                "08:00",
                "10:00"
            );
        }

        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = asignaturas[5].IdAsignatura,
            Dia = 6,
            HoraInicio = "08:00",
            HoraFin = "10:00",
            Periodo = "2026-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("límite de 5");
    }

    [Fact(DisplayName = "REQ 5 - Varios bloques de la misma asignatura cuentan como una sola materia")]
    public async Task Req5_MismaAsignaturaVariosBloques_NoAumentaCarga()
    {
        var docente = await CrearDocenteAsync("TP");

        var a1 = await CrearAsignaturaAsync("BLOQ001");
        var a2 = await CrearAsignaturaAsync("BLOQ002");
        var a3 = await CrearAsignaturaAsync("BLOQ003");

        await CrearAsignacionAsync(docente.IdProfesor, a1.IdAsignatura, 1, "08:00", "10:00");
        await CrearAsignacionAsync(docente.IdProfesor, a2.IdAsignatura, 2, "08:00", "10:00");
        await CrearAsignacionAsync(docente.IdProfesor, a3.IdAsignatura, 3, "08:00", "10:00");

        var segundoBloqueMismaMateria = new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = a1.IdAsignatura,
            Dia = 4,
            HoraInicio = "10:00",
            HoraFin = "12:00",
            Periodo = "2026-1"
        };

        var response = await Client.PostAsJsonAsync("/api/asignaciones", segundoBloqueMismaMateria);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignacion = await response.Content.ReadFromJsonAsync<AsignacionResponse>();
        asignacion!.AsignaturasActuales.Should().Be(3);
        asignacion.MaxAsignaturas.Should().Be(3);
    }

    [Fact(DisplayName = "POST /api/asignaciones - Rechaza bloque duplicado")]
    public async Task Crear_RechazaBloqueDuplicado()
    {
        var docente = await CrearDocenteAsync("TC");
        var asignatura = await CrearAsignaturaAsync("DUPBLOQ");

        await CrearAsignacionAsync(docente.IdProfesor, asignatura.IdAsignatura, 1, "08:00", "10:00");

        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = asignatura.IdAsignatura,
            Dia = 1,
            HoraInicio = "08:00",
            HoraFin = "10:00",
            Periodo = "2026-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("ya existe");
    }

    [Fact(DisplayName = "POST /api/asignaciones - Rechaza hora inicio mayor o igual a hora fin")]
    public async Task Crear_RechazaHoraInvalida()
    {
        var docente = await CrearDocenteAsync("TC");
        var asignatura = await CrearAsignaturaAsync("HORA001");

        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = asignatura.IdAsignatura,
            Dia = 1,
            HoraInicio = "10:00",
            HoraFin = "08:00",
            Periodo = "2026-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("hora de inicio");
    }

    [Fact(DisplayName = "GET /api/asignaciones/docente/{id}/resumen - Retorna resumen de carga")]
    public async Task ObtenerResumen_RetornaCargaDocente()
    {
        var docente = await CrearDocenteAsync("TP");
        var a1 = await CrearAsignaturaAsync("RES001");
        var a2 = await CrearAsignaturaAsync("RES002");

        await CrearAsignacionAsync(docente.IdProfesor, a1.IdAsignatura, 1, "08:00", "10:00");
        await CrearAsignacionAsync(docente.IdProfesor, a2.IdAsignatura, 2, "08:00", "10:00");

        var response = await Client.GetAsync($"/api/asignaciones/docente/{docente.IdProfesor}/resumen?periodo=2026-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var resumen = await response.Content.ReadFromJsonAsync<ResumenCargaDocenteResponse>();
        resumen!.IdDocente.Should().Be(docente.IdProfesor);
        resumen.TipoContrato.Should().Be("TP");
        resumen.MaxAsignaturas.Should().Be(3);
        resumen.AsignaturasActuales.Should().Be(2);
        resumen.PuedeAsignarMas.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/asignaciones/docente/{id} - Retorna asignaciones del docente")]
    public async Task ObtenerPorDocente_RetornaAsignaciones()
    {
        var docente = await CrearDocenteAsync("TC");
        var asignatura = await CrearAsignaturaAsync("GETDOC");

        await CrearAsignacionAsync(docente.IdProfesor, asignatura.IdAsignatura, 1, "08:00", "10:00");

        var response = await Client.GetAsync($"/api/asignaciones/docente/{docente.IdProfesor}?periodo=2026-1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var asignaciones = await response.Content.ReadFromJsonAsync<List<AsignacionResponse>>();
        asignaciones.Should().HaveCount(1);
        asignaciones![0].IdDocente.Should().Be(docente.IdProfesor);
    }

    [Fact(DisplayName = "DELETE /api/asignaciones/{id} - Elimina asignación")]
    public async Task Eliminar_Asignacion()
    {
        var docente = await CrearDocenteAsync("TC");
        var asignatura = await CrearAsignaturaAsync("DELASIG");

        var asignacion = await CrearAsignacionAsync(docente.IdProfesor, asignatura.IdAsignatura, 1, "08:00", "10:00");

        var response = await Client.DeleteAsync($"/api/asignaciones/{asignacion.IdAsignacion}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/asignaciones/docente/{docente.IdProfesor}?periodo=2026-1");
        var asignaciones = await getResponse.Content.ReadFromJsonAsync<List<AsignacionResponse>>();

        asignaciones.Should().BeEmpty();
    }

    private async Task<ProfesorResponse> CrearDocenteAsync(string tipoContrato)
    {
        var response = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = $"Docente {Guid.NewGuid().ToString("N")[..8]}",
            Identificacion = Guid.NewGuid().ToString("N")[..12],
            TipoContrato = tipoContrato
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<ProfesorResponse>())!;
    }

    private async Task<AsignaturaResponse> CrearAsignaturaAsync(string codigo)
    {
        var response = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = codigo,
            Nombre = $"Asignatura {codigo}",
            Creditos = 3,
            Semestre = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<AsignaturaResponse>())!;
    }

    private async Task<AsignacionResponse> CrearAsignacionAsync(
        string idDocente,
        string idAsignatura,
        int dia,
        string horaInicio,
        string horaFin)
    {
        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = idDocente,
            IdAsignatura = idAsignatura,
            Dia = dia,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Periodo = "2026-1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<AsignacionResponse>())!;
    }
}