using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Suite completa de pruebas para los 5 endpoints de Profesores API.
/// Pruebas independientes siguiendo patrón AAA.
/// </summary>
public class ProfesoresControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public ProfesoresControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region GET /api/profesores

    [Fact(DisplayName = "GET /api/profesores - Retorna lista vacía inicialmente")]
    public async Task ObtenerTodos_RetornListaVacia_CuandoNoHayProfesores()
    {
        // Arrange: BD limpia

        // Act
        var response = await Client.GetAsync("/api/profesores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profesores = await response.Content.ReadFromJsonAsync<List<ProfesorResponse>>();
        profesores.Should().NotBeNull();
        profesores.Should().BeEmpty();
    }

    [Fact(DisplayName = "GET /api/profesores - Retorna lista de todos los profesores")]
    public async Task ObtenerTodos_RetornListaProfesores_CuandoHayMultiples()
    {
        // Arrange: Crear 3 profesores
        var profesoresACrear = new[]
        {
            new CrearProfesorRequest { Nombre = "Dr. Carlos López", Identificacion = "12345678", TipoContrato = "Tiempo Completo" },
            new CrearProfesorRequest { Nombre = "Dra. María García", Identificacion = "87654321", TipoContrato = "Parcial" },
            new CrearProfesorRequest { Nombre = "Prof. Juan Rodríguez", Identificacion = "55555555", TipoContrato = "Tiempo Completo" }
        };

        foreach (var request in profesoresACrear)
        {
            await Client.PostAsJsonAsync("/api/profesores", request);
        }

        // Act
        var response = await Client.GetAsync("/api/profesores");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profesores = await response.Content.ReadFromJsonAsync<List<ProfesorResponse>>();
        profesores.Should().HaveCount(3);
    }

    #endregion

    #region GET /api/profesores/{idProfesor}

    [Fact(DisplayName = "GET /api/profesores/{id} - Retorna profesor existente")]
    public async Task ObtenerPorId_RetornProfesor_CuandoIdExiste()
    {
        // Arrange: Crear profesor
        var crearRequest = new CrearProfesorRequest
        {
            Nombre = "Dr. Pedro Sánchez",
            Identificacion = "99887766",
            TipoContrato = "Tiempo Completo"
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", crearRequest);
        var profesorCreado = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Act
        var getResponse = await Client.GetAsync($"/api/profesores/{profesorCreado!.IdProfesor}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var profesorObtenido = await getResponse.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesorObtenido.Should().NotBeNull();
        profesorObtenido!.IdProfesor.Should().Be(profesorCreado.IdProfesor);
        profesorObtenido.Nombre.Should().Be("Dr. Pedro Sánchez");
        profesorObtenido.Identificacion.Should().Be("99887766");
        profesorObtenido.TipoContrato.Should().Be("Tiempo Completo");
    }

    [Fact(DisplayName = "GET /api/profesores/{id} - Retorna 404 cuando no existe")]
    public async Task ObtenerPorId_Retorn404_CuandoIdNoExiste()
    {
        // Arrange

        // Act
        var response = await Client.GetAsync("/api/profesores/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/profesores

    [Fact(DisplayName = "POST /api/profesores - Crea profesor válido exitosamente")]
    public async Task Crear_RetornCreated_ConDatosValidos()
    {
        // Arrange
        var request = new CrearProfesorRequest
        {
            Nombre = "Prof. Ana Martínez",
            Identificacion = "11223344",
            TipoContrato = "Tiempo Completo"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var profesorCreado = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesorCreado.Should().NotBeNull();
        profesorCreado!.Nombre.Should().Be("Prof. Ana Martínez");
        profesorCreado.Identificacion.Should().Be("11223344");
        profesorCreado.TipoContrato.Should().Be("Tiempo Completo");
    }

    [Fact(DisplayName = "POST /api/profesores - Rechaza identificación duplicada")]
    public async Task Crear_RetornBadRequest_CuandoIdentificacionDuplicada()
    {
        // Arrange: Crear primer profesor
        var identificacion = "44332211";
        var request1 = new CrearProfesorRequest
        {
            Nombre = "Profesor 1",
            Identificacion = identificacion,
            TipoContrato = "Tiempo Completo"
        };
        await Client.PostAsJsonAsync("/api/profesores", request1);

        // Intentar crear segundo con misma identificación
        var request2 = new CrearProfesorRequest
        {
            Nombre = "Profesor 2",
            Identificacion = identificacion,
            TipoContrato = "Parcial"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/profesores", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.ReadAsStringAsync().Result.Should().Contain("identificaci");
    }

    [Theory(DisplayName = "POST /api/profesores - Valida requerimientos de entrada")]
    [InlineData("", "12345678", "Tiempo Completo")]
    [InlineData("Profesor", "", "Tiempo Completo")]
    [InlineData("Profesor", "12345678", "")]
    [InlineData("Profesor", "12345678", "Invalido")]
    public async Task Crear_RetornBadRequest_ConValidacionesFallidas(
        string nombre, string identificacion, string tipoContrato)
    {
        // Arrange
        var request = new CrearProfesorRequest
        {
            Nombre = nombre,
            Identificacion = identificacion,
            TipoContrato = tipoContrato
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory(DisplayName = "POST /api/profesores - Valida tipo de contrato")]
    [InlineData("Tiempo Completo")]
    [InlineData("Parcial")]
    public async Task Crear_AceptaTiposContratoValidos(string tipoContrato)
    {
        // Arrange
        var request = new CrearProfesorRequest
        {
            Nombre = "Prof. Test",
            Identificacion = $"ID{Guid.NewGuid().ToString().Substring(0, 8)}",
            TipoContrato = tipoContrato
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/profesores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor!.TipoContrato.Should().Be(tipoContrato);
    }

    #endregion

    #region PUT /api/profesores/{idProfesor}

    [Fact(DisplayName = "PUT /api/profesores/{id} - Actualiza profesor exitosamente")]
    public async Task Actualizar_RetornNoContent_ConDatosValidos()
    {
        // Arrange: Crear profesor
        var crearRequest = new CrearProfesorRequest
        {
            Nombre = "Nombre Original",
            Identificacion = "10101010",
            TipoContrato = "Tiempo Completo"
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", crearRequest);
        var profesor = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Actualizar
        var actualizarRequest = new ActualizarProfesorRequest
        {
            Nombre = "Nombre Actualizado",
            Identificacion = "20202020",
            TipoContrato = "Parcial"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/profesores/{profesor!.IdProfesor}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar cambios
        var getResponse = await Client.GetAsync($"/api/profesores/{profesor.IdProfesor}");
        var profesorActualizado = await getResponse.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesorActualizado!.Nombre.Should().Be("Nombre Actualizado");
        profesorActualizado.Identificacion.Should().Be("20202020");
        profesorActualizado.TipoContrato.Should().Be("Parcial");
    }

    [Fact(DisplayName = "PUT /api/profesores/{id} - Retorna 404 cuando no existe")]
    public async Task Actualizar_Retorn404_CuandoProfesorNoExiste()
    {
        // Arrange
        var request = new ActualizarProfesorRequest
        {
            Nombre = "Test",
            Identificacion = "99999999",
            TipoContrato = "Tiempo Completo"
        };

        // Act
        var response = await Client.PutAsJsonAsync("/api/profesores/9999", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/profesores/{id} - Rechaza identificación duplicada")]
    public async Task Actualizar_RetornBadRequest_CuandoIdentificacionDuplicada()
    {
        // Arrange: Crear dos profesores
        var prof1Request = new CrearProfesorRequest
        {
            Nombre = "Profesor 1",
            Identificacion = "11111111",
            TipoContrato = "Tiempo Completo"
        };
        var prof1Response = await Client.PostAsJsonAsync("/api/profesores", prof1Request);
        var prof1 = await prof1Response.Content.ReadFromJsonAsync<ProfesorResponse>();

        var prof2Request = new CrearProfesorRequest
        {
            Nombre = "Profesor 2",
            Identificacion = "22222222",
            TipoContrato = "Tiempo Completo"
        };
        var prof2Response = await Client.PostAsJsonAsync("/api/profesores", prof2Request);

        // Intentar actualizar prof1 con identificación de prof2
        var actualizarRequest = new ActualizarProfesorRequest
        {
            Nombre = "Profesor 1 Modificado",
            Identificacion = "22222222",
            TipoContrato = "Tiempo Completo"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/profesores/{prof1!.IdProfesor}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "PUT /api/profesores/{id} - Permite mantener propia identificación")]
    public async Task Actualizar_PermiteIdPropia_CuandoActualizaOtrosCampos()
    {
        // Arrange: Crear profesor
        var crearRequest = new CrearProfesorRequest
        {
            Nombre = "Profesor Original",
            Identificacion = "33333333",
            TipoContrato = "Tiempo Completo"
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", crearRequest);
        var profesor = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Actualizar con misma identificación pero otro nombre
        var actualizarRequest = new ActualizarProfesorRequest
        {
            Nombre = "Profesor Nuevo Nombre",
            Identificacion = "33333333",
            TipoContrato = "Parcial"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/profesores/{profesor!.IdProfesor}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar cambios
        var getResponse = await Client.GetAsync($"/api/profesores/{profesor.IdProfesor}");
        var profesorActualizado = await getResponse.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesorActualizado!.Nombre.Should().Be("Profesor Nuevo Nombre");
        profesorActualizado.Identificacion.Should().Be("33333333");
    }

    #endregion

    #region DELETE /api/profesores/{idProfesor}

    [Fact(DisplayName = "DELETE /api/profesores/{id} - Elimina profesor exitosamente")]
    public async Task Eliminar_RetornNoContent_CuandoExiste()
    {
        // Arrange: Crear profesor
        var crearRequest = new CrearProfesorRequest
        {
            Nombre = "Profesor a Eliminar",
            Identificacion = "77777777",
            TipoContrato = "Tiempo Completo"
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/profesores", crearRequest);
        var profesor = await crearResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Act
        var response = await Client.DeleteAsync($"/api/profesores/{profesor!.IdProfesor}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que no existe
        var getResponse = await Client.GetAsync($"/api/profesores/{profesor.IdProfesor}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/profesores/{id} - Retorna 404 cuando no existe")]
    public async Task Eliminar_Retorn404_CuandoNoExiste()
    {
        // Arrange

        // Act
        var response = await Client.DeleteAsync("/api/profesores/9999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
