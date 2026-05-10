using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

public class ProfesoresControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public ProfesoresControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "GET /api/profesores - Retorna lista vacía inicialmente")]
    public async Task ObtenerTodos_RetornaListaVacia()
    {
        var response = await Client.GetAsync("/api/profesores");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var profesores = await response.Content.ReadFromJsonAsync<List<ProfesorResponse>>();
        profesores.Should().NotBeNull();
        profesores.Should().BeEmpty();
    }

    [Fact(DisplayName = "POST /api/profesores - Crea docente TC con máximo 5 asignaturas")]
    public async Task Crear_DocenteTC_AsignaMaximoCinco()
    {
        var request = new CrearProfesorRequest
        {
            Nombre = "Carlos Pérez",
            Identificacion = "1001",
            TipoContrato = "TC"
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor.Should().NotBeNull();
        profesor!.IdProfesor.Should().NotBeNullOrWhiteSpace();
        profesor.Nombre.Should().Be("Carlos Pérez");
        profesor.Identificacion.Should().Be("1001");
        profesor.TipoContrato.Should().Be("TC");
        profesor.MaxAsignaturas.Should().Be(5);
    }

    [Fact(DisplayName = "POST /api/profesores - Crea docente TP con máximo 3 asignaturas")]
    public async Task Crear_DocenteTP_AsignaMaximoTres()
    {
        var request = new CrearProfesorRequest
        {
            Nombre = "Ana Gómez",
            Identificacion = "1002",
            TipoContrato = "TP"
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor.Should().NotBeNull();
        profesor!.TipoContrato.Should().Be("TP");
        profesor.MaxAsignaturas.Should().Be(3);
    }

    [Theory(DisplayName = "POST /api/profesores - Normaliza contratos largos a TC o TP")]
    [InlineData("Tiempo Completo", "TC", 5)]
    [InlineData("Parcial", "TP", 3)]
    public async Task Crear_NormalizaTipoContrato(string entrada, string esperado, int maxEsperado)
    {
        var request = new CrearProfesorRequest
        {
            Nombre = $"Docente {entrada}",
            Identificacion = Guid.NewGuid().ToString("N")[..8],
            TipoContrato = entrada
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor!.TipoContrato.Should().Be(esperado);
        profesor.MaxAsignaturas.Should().Be(maxEsperado);
    }

    [Fact(DisplayName = "GET /api/profesores/{id} - Obtiene docente existente")]
    public async Task ObtenerPorId_RetornaDocenteExistente()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Pedro Sánchez",
            Identificacion = "2001",
            TipoContrato = "TC"
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        var response = await Client.GetAsync($"/api/profesores/{creado!.IdProfesor}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var obtenido = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        obtenido!.IdProfesor.Should().Be(creado.IdProfesor);
        obtenido.Nombre.Should().Be("Pedro Sánchez");
        obtenido.MaxAsignaturas.Should().Be(5);
    }

    [Fact(DisplayName = "GET /api/profesores/{id} - Retorna 404 si no existe")]
    public async Task ObtenerPorId_Retorna404()
    {
        var response = await Client.GetAsync($"/api/profesores/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "POST /api/profesores - Rechaza identificación duplicada")]
    public async Task Crear_RechazaIdentificacionDuplicada()
    {
        var request1 = new CrearProfesorRequest
        {
            Nombre = "Docente Uno",
            Identificacion = "3001",
            TipoContrato = "TC"
        };

        var request2 = new CrearProfesorRequest
        {
            Nombre = "Docente Dos",
            Identificacion = "3001",
            TipoContrato = "TP"
        };

        await Client.PostAsJsonAsync("/api/profesores", request1);

        var response = await Client.PostAsJsonAsync("/api/profesores", request2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("identificación");
    }

    [Theory(DisplayName = "POST /api/profesores - Valida campos obligatorios")]
    [InlineData("", "4001", "TC")]
    [InlineData("Docente", "", "TC")]
    [InlineData("Docente", "4001", "")]
    [InlineData("Docente", "4001", "Invalido")]
    public async Task Crear_RechazaDatosInvalidos(string nombre, string identificacion, string tipoContrato)
    {
        var request = new CrearProfesorRequest
        {
            Nombre = nombre,
            Identificacion = identificacion,
            TipoContrato = tipoContrato
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PUT /api/profesores/{id} - Actualiza docente y recalcula máximo")]
    public async Task Actualizar_Docente_RecalculaMaximo()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Docente Original",
            Identificacion = "5001",
            TipoContrato = "TC"
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        var actualizarRequest = new ActualizarProfesorRequest
        {
            Nombre = "Docente Actualizado",
            Identificacion = "5002",
            TipoContrato = "TP"
        };

        var response = await Client.PutAsJsonAsync($"/api/profesores/{creado!.IdProfesor}", actualizarRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/profesores/{creado.IdProfesor}");
        var actualizado = await getResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        actualizado!.Nombre.Should().Be("Docente Actualizado");
        actualizado.Identificacion.Should().Be("5002");
        actualizado.TipoContrato.Should().Be("TP");
        actualizado.MaxAsignaturas.Should().Be(3);
    }

    [Fact(DisplayName = "DELETE /api/profesores/{id} - Elimina docente sin asignaciones")]
    public async Task Eliminar_DocenteSinAsignaciones()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Docente Eliminable",
            Identificacion = "6001",
            TipoContrato = "TP"
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        var deleteResponse = await Client.DeleteAsync($"/api/profesores/{creado!.IdProfesor}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/profesores/{creado.IdProfesor}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}