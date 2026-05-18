using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.DTOs.Bloqueos;
using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

public class BloqueosFranjaAsignaturaControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    private const string PlanDiurno = "11111111-1111-1111-1111-111111111111";
    private const string Periodo = "2026-1";

    public BloqueosFranjaAsignaturaControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "REQ 36 - Crea, consulta y elimina bloqueo de franja por asignatura")]
    public async Task Req36_CrearConsultarEliminarBloqueo()
    {
        AsignaturaResponse asignatura = await CrearAsignaturaAsync("BLQ001", 2);

        var crearResponse = await Client.PostAsJsonAsync(
            $"/api/asignaturas/{asignatura.IdAsignatura}/bloqueos-franja",
            new CrearBloqueoFranjaAsignaturaRequest
            {
                Periodo = Periodo,
                Dia = 1,
                HoraInicio = "08:00",
                HoraFin = "10:00",
                Motivo = "Laboratorio reservado"
            }
        );

        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        BloqueoFranjaAsignaturaResponse bloqueo =
            (await crearResponse.Content.ReadFromJsonAsync<BloqueoFranjaAsignaturaResponse>())!;

        bloqueo.IdAsignatura.Should().Be(asignatura.IdAsignatura);
        bloqueo.Periodo.Should().Be(Periodo);
        bloqueo.Dia.Should().Be(1);
        bloqueo.HoraInicio.Should().Be("08:00");
        bloqueo.HoraFin.Should().Be("10:00");

        var listarResponse = await Client.GetAsync(
            $"/api/asignaturas/{asignatura.IdAsignatura}/bloqueos-franja?periodo={Periodo}"
        );

        listarResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        List<BloqueoFranjaAsignaturaResponse> bloqueos =
            (await listarResponse.Content.ReadFromJsonAsync<List<BloqueoFranjaAsignaturaResponse>>())!;

        bloqueos.Should().ContainSingle(b => b.IdBloqueo == bloqueo.IdBloqueo);

        var deleteResponse = await Client.DeleteAsync($"/api/asignaturas/bloqueos-franja/{bloqueo.IdBloqueo}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "REQ 36 - Rechaza asignación con horario dentro de franja bloqueada")]
    public async Task Req36_RechazaAsignacionEnFranjaBloqueada()
    {
        ProfesorResponse docente = await CrearDocenteAsync("TC");
        AsignaturaResponse asignatura = await CrearAsignaturaAsync("BLQ002", 2);

        await Client.PostAsJsonAsync(
            $"/api/asignaturas/{asignatura.IdAsignatura}/bloqueos-franja",
            new CrearBloqueoFranjaAsignaturaRequest
            {
                Periodo = Periodo,
                Dia = 1,
                HoraInicio = "08:00",
                HoraFin = "10:00",
                Motivo = "Franja no disponible para esta asignatura"
            }
        );

        var response = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = docente.IdProfesor,
            IdAsignatura = asignatura.IdAsignatura,
            Dia = 1,
            HoraInicio = "09:00",
            HoraFin = "11:00",
            Periodo = Periodo
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        string body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("bloqueada");
    }

    [Fact(DisplayName = "REQ 36 - El generador automático evita la franja bloqueada")]
    public async Task Req36_GeneradorEvitaFranjaBloqueada()
    {
        ProfesorResponse docente = await CrearDocenteAsync("TC");
        AsignaturaResponse asignatura = await CrearAsignaturaAsync("BLQ003", 2);

        await Factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.DocentesHabilitados.Add(new DocenteHabilitado
            {
                IdDocente = docente.IdProfesor,
                IdAsignatura = asignatura.IdAsignatura,
                Fuente = "Test"
            });

            dbContext.Disponibilidades.AddRange(
                new Disponibilidad
                {
                    IdDocente = docente.IdProfesor,
                    DiaSemana = 1,
                    HoraInicio = "08:00",
                    HoraFin = "12:00"
                },
                new Disponibilidad
                {
                    IdDocente = docente.IdProfesor,
                    DiaSemana = 2,
                    HoraInicio = "08:00",
                    HoraFin = "12:00"
                }
            );

            await dbContext.SaveChangesAsync();
        });

        await Client.PostAsJsonAsync(
            $"/api/asignaturas/{asignatura.IdAsignatura}/bloqueos-franja",
            new CrearBloqueoFranjaAsignaturaRequest
            {
                Periodo = Periodo,
                Dia = 1,
                HoraInicio = "08:00",
                HoraFin = "10:00",
                Motivo = "La asignatura no puede dictarse el lunes en la mañana"
            }
        );

        var response = await Client.PostAsJsonAsync("/api/horarios/generar-propuestas", new GenerarPropuestasHorarioRequest
        {
            Periodo = Periodo,
            Escenarios = new List<string> { "ING_DIURNA" },
            SemestreIngenieria = 1,
            BorrarPropuestasPrevias = true
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        GenerarPropuestasHorarioResponse resultado =
            (await response.Content.ReadFromJsonAsync<GenerarPropuestasHorarioResponse>())!;

        resultado.Propuestas.Should().ContainSingle();
        resultado.Propuestas[0].IdAsignatura.Should().Be(asignatura.IdAsignatura);
        resultado.Propuestas[0].Dia.Should().Be(2);
        resultado.Propuestas[0].HoraInicio.Should().Be("08:00");
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

    private async Task<AsignaturaResponse> CrearAsignaturaAsync(string codigo, int creditos)
    {
        var response = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = codigo,
            Nombre = $"Asignatura {codigo}",
            Creditos = creditos,
            Semestre = 1
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<AsignaturaResponse>())!;
    }
}