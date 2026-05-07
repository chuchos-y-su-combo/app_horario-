using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Suite completa de pruebas para los 5 endpoints de Asignaturas API.
/// Pruebas independientes siguiendo patrón AAA.
/// </summary>
public class AsignaturasControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public AsignaturasControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region GET /api/asignaturas

    [Fact(DisplayName = "GET /api/asignaturas - Retorna lista vacía inicialmente")]
    public async Task ObtenerTodas_RetornListaVacia_CuandoNoHayAsignaturas()
    {
        // Arrange: BD limpia

        // Act
        var response = await Client.GetAsync("/api/asignaturas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().NotBeNull();
        asignaturas.Should().BeEmpty();
    }

    [Fact(DisplayName = "GET /api/asignaturas - Retorna lista ordenada por semestre y nombre")]
    public async Task ObtenerTodas_RetornListaOrdenada_CuandoHayAsignaturas()
    {
        // Arrange: Crear asignaturas en orden aleatorio
        var asignaturasACrear = new[]
        {
            new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "MAT102", Nombre = "Cálculo II", Creditos = 4, Semestre = 2 },
            new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "MAT101", Nombre = "Cálculo I", Creditos = 4, Semestre = 1 },
            new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "FIS101", Nombre = "Física I", Creditos = 3, Semestre = 1 }
        };

        foreach (var request in asignaturasACrear)
        {
            await Client.PostAsJsonAsync("/api/asignaturas", request);
        }

        // Act
        var response = await Client.GetAsync("/api/asignaturas");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().HaveCount(3);
        // Verificar ordenamiento: por semestre, luego por nombre
        asignaturas![0].Semestre.Should().Be(1);
        asignaturas[1].Semestre.Should().Be(1);
        asignaturas[2].Semestre.Should().Be(2);
    }

    #endregion

    #region GET /api/asignaturas/plan/{idPlanEstudios}

    [Fact(DisplayName = "GET /api/asignaturas/plan/{id} - Retorna asignaturas del plan")]
    public async Task ObtenerPorPlanEstudios_RetornAsignaturas_CuandoPlanExiste()
    {
        // Arrange: Crear asignaturas para dos planes diferentes
        var plan1Asignaturas = new[]
        {
            new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "MAT101", Nombre = "Matemáticas I", Creditos = 3, Semestre = 1 },
            new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "FIS101", Nombre = "Física I", Creditos = 3, Semestre = 1 }
        };
        
        var plan2Asignaturas = new[]
        {
            new CrearAsignaturaRequest { IdPlanEstudios = 2, Codigo = "PROG101", Nombre = "Programación I", Creditos = 4, Semestre = 1 }
        };

        foreach (var request in plan1Asignaturas.Concat(plan2Asignaturas))
        {
            await Client.PostAsJsonAsync("/api/asignaturas", request);
        }

        // Act
        var response = await Client.GetAsync("/api/asignaturas/plan/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().HaveCount(2);
        asignaturas.Should().AllSatisfy(a => a.IdPlanEstudios.Should().Be(1));
    }

    [Fact(DisplayName = "GET /api/asignaturas/plan/{id} - Retorna lista vacía cuando no hay asignaturas")]
    public async Task ObtenerPorPlanEstudios_RetornListaVacia_CuandoPlanNoTieneAsignaturas()
    {
        // Arrange: Plan sin asignaturas

        // Act
        var response = await Client.GetAsync("/api/asignaturas/plan/999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().BeEmpty();
    }

    #endregion

    #region GET /api/asignaturas/{idAsignatura}

    [Fact(DisplayName = "GET /api/asignaturas/{id} - Retorna asignatura existente")]
    public async Task ObtenerPorId_RetornAsignatura_CuandoIdExiste()
    {
        // Arrange: Crear asignatura
        var crearRequest = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "MAT201",
            Nombre = "Álgebra Lineal",
            Creditos = 4,
            Semestre = 2
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/asignaturas", crearRequest);
        var asignaturaCreada = await crearResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Act
        var getResponse = await Client.GetAsync($"/api/asignaturas/{asignaturaCreada!.IdAsignatura}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturaObtenida = await getResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignaturaObtenida.Should().NotBeNull();
        asignaturaObtenida!.IdAsignatura.Should().Be(asignaturaCreada.IdAsignatura);
        asignaturaObtenida.Codigo.Should().Be("MAT201");
        asignaturaObtenida.Nombre.Should().Be("Álgebra Lineal");
    }

    [Fact(DisplayName = "GET /api/asignaturas/{id} - Retorna 404 cuando no existe")]
    public async Task ObtenerPorId_Retorn404_CuandoIdNoExiste()
    {
        // Arrange: ID que no existe

        // Act
        var response = await Client.GetAsync("/api/asignaturas/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/asignaturas

    [Fact(DisplayName = "POST /api/asignaturas - Crea asignatura válida exitosamente")]
    public async Task Crear_RetornCreated_ConDatosValidos()
    {
        // Arrange
        var request = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "BIO101",
            Nombre = "Biología General",
            Creditos = 3,
            Semestre = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var asignaturaCreada = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignaturaCreada.Should().NotBeNull();
        asignaturaCreada!.Codigo.Should().Be("BIO101");
        asignaturaCreada.Nombre.Should().Be("Biología General");
        asignaturaCreada.Creditos.Should().Be(3);
        asignaturaCreada.Semestre.Should().Be(1);
    }

    [Fact(DisplayName = "POST /api/asignaturas - Normaliza código a mayúsculas")]
    public async Task Crear_NormalizaCodigo_AMayusculas()
    {
        // Arrange
        var request = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "che201",
            Nombre = "Química",
            Creditos = 3,
            Semestre = 2
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);
        var asignaturaCreada = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Assert
        asignaturaCreada!.Codigo.Should().Be("CHE201");
    }

    [Fact(DisplayName = "POST /api/asignaturas - Rechaza código duplicado")]
    public async Task Crear_RetornBadRequest_CuandoCodigoDuplicado()
    {
        // Arrange: Crear primera asignatura
        var codigo = "HIST101";
        var request1 = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = codigo,
            Nombre = "Historia I",
            Creditos = 2,
            Semestre = 1
        };
        await Client.PostAsJsonAsync("/api/asignaturas", request1);

        // Intentar crear segunda con mismo código
        var request2 = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 2,
            Codigo = codigo,
            Nombre = "Historia II",
            Creditos = 2,
            Semestre = 2
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/asignaturas", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.ReadAsStringAsync().Result.Should().Contain("código");
    }

    [Theory(DisplayName = "POST /api/asignaturas - Valida rangos de créditos y semestre")]
    [InlineData(0, 1)]      // Créditos fuera de rango
    [InlineData(21, 1)]     // Créditos fuera de rango
    [InlineData(3, 0)]      // Semestre fuera de rango
    [InlineData(3, 11)]     // Semestre fuera de rango
    public async Task Crear_RetornBadRequest_ConRangosFueraDelLimite(int creditos, int semestre)
    {
        // Arrange
        var request = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "TEST100",
            Nombre = "Test",
            Creditos = creditos,
            Semestre = semestre
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/asignaturas/{idAsignatura}

    [Fact(DisplayName = "PUT /api/asignaturas/{id} - Actualiza asignatura exitosamente")]
    public async Task Actualizar_RetornNoContent_ConDatosValidos()
    {
        // Arrange: Crear asignatura
        var crearRequest = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "ORIGINAL",
            Nombre = "Nombre Original",
            Creditos = 2,
            Semestre = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/asignaturas", crearRequest);
        var asignatura = await crearResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Actualizar
        var actualizarRequest = new ActualizarAsignaturaRequest
        {
            IdPlanEstudios = 2,
            Codigo = "UPDATED",
            Nombre = "Nombre Actualizado",
            Creditos = 4,
            Semestre = 3
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/asignaturas/{asignatura!.IdAsignatura}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar cambios
        var getResponse = await Client.GetAsync($"/api/asignaturas/{asignatura.IdAsignatura}");
        var asignaturaActualizada = await getResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignaturaActualizada!.Codigo.Should().Be("UPDATED");
        asignaturaActualizada.Nombre.Should().Be("Nombre Actualizado");
        asignaturaActualizada.Creditos.Should().Be(4);
    }

    [Fact(DisplayName = "PUT /api/asignaturas/{id} - Retorna 404 cuando no existe")]
    public async Task Actualizar_Retorn404_CuandoAsignaturaNoExiste()
    {
        // Arrange
        var request = new ActualizarAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "TEST",
            Nombre = "Test",
            Creditos = 3,
            Semestre = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/asignaturas/9999", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/asignaturas/{id} - Rechaza código duplicado en otra asignatura")]
    public async Task Actualizar_RetornBadRequest_CuandoCodigoDuplicado()
    {
        // Arrange: Crear dos asignaturas
        var asig1 = new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "ASIG001", Nombre = "Asignatura 1", Creditos = 3, Semestre = 1 };
        var asig1Response = await Client.PostAsJsonAsync("/api/asignaturas", asig1);
        var asig1Created = await asig1Response.Content.ReadFromJsonAsync<AsignaturaResponse>();

        var asig2 = new CrearAsignaturaRequest { IdPlanEstudios = 1, Codigo = "ASIG002", Nombre = "Asignatura 2", Creditos = 3, Semestre = 1 };
        var asig2Response = await Client.PostAsJsonAsync("/api/asignaturas", asig2);

        // Intentar cambiar código de asig1 al código de asig2
        var actualizarRequest = new ActualizarAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "ASIG002",
            Nombre = "Asignatura 1 Modificada",
            Creditos = 3,
            Semestre = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/asignaturas/{asig1Created!.IdAsignatura}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region DELETE /api/asignaturas/{idAsignatura}

    [Fact(DisplayName = "DELETE /api/asignaturas/{id} - Elimina asignatura exitosamente")]
    public async Task Eliminar_RetornNoContent_CuandoExiste()
    {
        // Arrange: Crear asignatura
        var crearRequest = new CrearAsignaturaRequest
        {
            IdPlanEstudios = 1,
            Codigo = "ELIM101",
            Nombre = "A Eliminar",
            Creditos = 2,
            Semestre = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/asignaturas", crearRequest);
        var asignatura = await crearResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Act
        var response = await Client.DeleteAsync($"/api/asignaturas/{asignatura!.IdAsignatura}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que no existe
        var getResponse = await Client.GetAsync($"/api/asignaturas/{asignatura.IdAsignatura}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/asignaturas/{id} - Retorna 404 cuando no existe")]
    public async Task Eliminar_Retorn404_CuandoNoExiste()
    {
        // Arrange

        // Act
        var response = await Client.DeleteAsync("/api/asignaturas/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
