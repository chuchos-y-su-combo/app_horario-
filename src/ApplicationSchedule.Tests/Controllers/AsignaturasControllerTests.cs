using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Pruebas de integración para el controlador de asignaturas.
/// </summary>
public class AsignaturasControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    private const string PlanDiurno = "11111111-1111-1111-1111-111111111111";
    private const string PlanNocturno = "22222222-2222-2222-2222-222222222222";

    public AsignaturasControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "GET /api/asignaturas - Retorna lista vacía inicialmente")]
    public async Task ObtenerTodas_RetornaListaVacia()
    {
        var response = await Client.GetAsync("/api/asignaturas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().NotBeNull();
        asignaturas.Should().BeEmpty();
    }

    [Fact(DisplayName = "POST /api/asignaturas - Crea asignatura válida con plan oficial")]
    public async Task Crear_AsignaturaValida()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "BIO101",
            Nombre = "Biología General",
            Creditos = 3,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura.Should().NotBeNull();
        asignatura!.IdAsignatura.Should().NotBeNullOrWhiteSpace();
        asignatura.IdPlan.Should().Be(PlanDiurno);
        asignatura.Codigo.Should().Be("BIO101");
        asignatura.Nombre.Should().Be("Biología General");
        asignatura.Creditos.Should().Be(3);
        asignatura.Semestre.Should().Be(1);
        asignatura.MinEstudiantes.Should().Be(15);
        asignatura.EsFijaTapsi.Should().BeFalse();
    }

    [Fact(DisplayName = "POST /api/asignaturas - Rechaza plan inexistente")]
    public async Task Crear_RechazaPlanInexistente()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = Guid.NewGuid().ToString(),
            Codigo = "PLAN404",
            Nombre = "Materia sin plan",
            Creditos = 3,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("plan de estudios");
    }

    [Fact(DisplayName = "POST /api/asignaturas - Normaliza código a mayúsculas")]
    public async Task Crear_NormalizaCodigo()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "abc123",
            Nombre = "Materia Test",
            Creditos = 3,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.Codigo.Should().Be("ABC123");
    }

    [Fact(DisplayName = "POST /api/asignaturas - Rechaza código duplicado")]
    public async Task Crear_RechazaCodigoDuplicado()
    {
        var request1 = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "DUP101",
            Nombre = "Materia Uno",
            Creditos = 3,
            Semestre = 1
        };

        var request2 = new CrearAsignaturaRequest
        {
            IdPlan = PlanNocturno,
            Codigo = "DUP101",
            Nombre = "Materia Dos",
            Creditos = 3,
            Semestre = 2
        };

        await Client.PostAsJsonAsync("/api/asignaturas", request1);

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("código");
    }

    [Fact(DisplayName = "REQ 7 - Marca automáticamente materia TAPSI fija por código")]
    public async Task Crear_MateriaTapsi_PorCodigo_QuedaFija()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "103007",
            Nombre = "Técnicas de programación",
            Creditos = 3,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.EsFijaTapsi.Should().BeTrue();
    }

    [Fact(DisplayName = "REQ 7 - Marca automáticamente materia TAPSI fija por nombre sin tilde")]
    public async Task Crear_MateriaTapsi_PorNombreSinTilde_QuedaFija()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "TEST999",
            Nombre = "Calculo diferencial",
            Creditos = 3,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.EsFijaTapsi.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/asignaturas/tapsi/fijas - Retorna solo materias fijas")]
    public async Task ObtenerFijasTapsi_RetornaSoloFijas()
    {
        await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "103007",
            Nombre = "Técnicas de programación",
            Creditos = 3,
            Semestre = 1
        });

        await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "NORMAL101",
            Nombre = "Materia Normal",
            Creditos = 3,
            Semestre = 1
        });

        var response = await Client.GetAsync("/api/asignaturas/tapsi/fijas");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().HaveCount(1);
        asignaturas![0].Codigo.Should().Be("103007");
        asignaturas[0].EsFijaTapsi.Should().BeTrue();
    }

    [Fact(DisplayName = "POST /api/asignaturas/tapsi/marcar-fijas - Marca TAPSI existentes")]
    public async Task MarcarFijas_MarcaAsignaturasExistentes()
    {
        await Factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Asignaturas.Add(new Asignatura
            {
                IdAsignatura = Guid.NewGuid().ToString(),
                IdPlan = PlanDiurno,
                Codigo = "103004",
                Nombre = "Teoría de sistemas",
                Creditos = 3,
                Semestre = 1,
                MinEstudiantes = 15,
                EsFijaTapsi = false
            });

            await dbContext.SaveChangesAsync();
        });

        var response = await Client.PostAsync("/api/asignaturas/tapsi/marcar-fijas", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var fijasResponse = await Client.GetAsync("/api/asignaturas/tapsi/fijas");
        var fijas = await fijasResponse.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();

        fijas.Should().HaveCount(1);
        fijas![0].Codigo.Should().Be("103004");
        fijas[0].EsFijaTapsi.Should().BeTrue();
    }

    [Fact(DisplayName = "GET /api/asignaturas/plan/{idPlan} - Retorna asignaturas del plan")]
    public async Task ObtenerPorPlan_RetornaAsignaturasDelPlan()
    {
        await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "DIU101",
            Nombre = "Materia Diurna",
            Creditos = 3,
            Semestre = 1
        });

        await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanNocturno,
            Codigo = "NOC101",
            Nombre = "Materia Nocturna",
            Creditos = 3,
            Semestre = 1
        });

        var response = await Client.GetAsync($"/api/asignaturas/plan/{PlanDiurno}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().HaveCount(1);
        asignaturas![0].IdPlan.Should().Be(PlanDiurno);
        asignaturas[0].Codigo.Should().Be("DIU101");
    }

    [Fact(DisplayName = "DELETE /api/asignaturas/{id} - Rechaza eliminar materia TAPSI fija")]
    public async Task Eliminar_RechazaMateriaTapsiFija()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "103027",
            Nombre = "Sistemas operativos",
            Creditos = 3,
            Semestre = 3
        });

        var asignatura = await crearResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        var response = await Client.DeleteAsync($"/api/asignaturas/{asignatura!.IdAsignatura}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("TAPSI");
    }

    [Fact(DisplayName = "DELETE /api/asignaturas/{id} - Elimina materia normal")]
    public async Task Eliminar_MateriaNormal()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "DEL101",
            Nombre = "Materia Eliminable",
            Creditos = 3,
            Semestre = 1
        });

        var asignatura = await crearResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        var response = await Client.DeleteAsync($"/api/asignaturas/{asignatura!.IdAsignatura}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/asignaturas/{asignatura.IdAsignatura}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}