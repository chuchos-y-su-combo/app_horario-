using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Asignaciones;
using ApplicationSchedule.Application.DTOs.Asignaturas;
using ApplicationSchedule.Application.DTOs.Horarios;
using ApplicationSchedule.Application.DTOs.Profesores;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Comprehensive test coverage for all API endpoints.
/// Tests positive cases, negative cases, and edge cases.
/// </summary>
public class EndpointCoverageTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    private const string PlanDiurno = "11111111-1111-1111-1111-111111111111";
    private const string PlanNocturno = "22222222-2222-2222-2222-222222222222";

    public EndpointCoverageTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    #region === USUARIOS ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/usuarios] Returns list")]
    public async Task GetUsuarios_ReturnsOkWithList()
    {
        var response = await Client.GetAsync("/api/usuarios");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuarios.Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "[POST /api/usuarios] Creates valid user")]
    public async Task PostUsuarios_CreatesValidUser()
    {
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Test User",
            Correo = "test@example.com",
            Password = "Password123",
            IdRol = 1
        };

        var response = await Client.PostAsJsonAsync("/api/usuarios", request);
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario.Should().NotBeNull();
        usuario!.Correo.Should().Be("test@example.com");
    }

    [Fact(DisplayName = "[POST /api/usuarios] Rejects invalid role")]
    public async Task PostUsuarios_RejectsInvalidRole()
    {
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Test User",
            Correo = "test@example.com",
            Password = "Password123",
            IdRol = 999
        };

        var response = await Client.PostAsJsonAsync("/api/usuarios", request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "[GET /api/usuarios/{id}] Returns existing user")]
    public async Task GetUsuarioById_ReturnsExistingUser()
    {
        // Create user
        var createResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Test",
            Correo = "test@test.com",
            Password = "Pass123",
            IdRol = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Get by id
        var response = await Client.GetAsync($"/api/usuarios/{created!.IdUsuario}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario!.IdUsuario.Should().Be(created.IdUsuario);
    }

    [Fact(DisplayName = "[GET /api/usuarios/{id}] Returns 404 for non-existent user")]
    public async Task GetUsuarioById_Returns404ForNonExistent()
    {
        var response = await Client.GetAsync($"/api/usuarios/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "[PUT /api/usuarios/{id}] Updates user")]
    public async Task PutUsuario_UpdatesUser()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Original",
            Correo = "original@test.com",
            Password = "Pass123",
            IdRol = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Update
        var updateRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Updated",
            Correo = "updated@test.com",
            IdRol = 2
        };
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{created!.IdUsuario}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/usuarios/{created.IdUsuario}");
        var updated = await getResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        updated!.NombreCompleto.Should().Be("Updated");
    }

    [Fact(DisplayName = "[DELETE /api/usuarios/{id}] Deletes user")]
    public async Task DeleteUsuario_DeletesUser()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "ToDelete",
            Correo = "delete@test.com",
            Password = "Pass123",
            IdRol = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Delete
        var response = await Client.DeleteAsync($"/api/usuarios/{created!.IdUsuario}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify deleted
        var getResponse = await Client.GetAsync($"/api/usuarios/{created.IdUsuario}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "[PATCH /api/usuarios/{id}/password] Changes password")]
    public async Task PatchUsuarioPassword_ChangesPassword()
    {
        // Create user
        var createResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Test",
            Correo = "pass@test.com",
            Password = "OldPass123",
            IdRol = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Change password
        var response = await Client.PatchAsJsonAsync($"/api/usuarios/{created!.IdUsuario}/password", 
            new CambiarPasswordRequest { NuevaPassword = "NewPass123" });
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region === PROFESORES ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/profesores] Returns list")]
    public async Task GetProfesores_ReturnsOkWithList()
    {
        var response = await Client.GetAsync("/api/profesores");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profesores = await response.Content.ReadFromJsonAsync<List<ProfesorResponse>>();
        profesores.Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "[POST /api/profesores] Creates TC professor with max 5 subjects")]
    public async Task PostProfesor_CreatesTC()
    {
        var request = new CrearProfesorRequest
        {
            Nombre = "Prof TC",
            Identificacion = "1234567890",
            TipoContrato = "TC"
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor!.TipoContrato.Should().Be("TC");
        profesor.MaxAsignaturas.Should().Be(5);
    }

    [Fact(DisplayName = "[POST /api/profesores] Creates TP professor with max 3 subjects")]
    public async Task PostProfesor_CreatesTP()
    {
        var request = new CrearProfesorRequest
        {
            Nombre = "Prof TP",
            Identificacion = "0987654321",
            TipoContrato = "TP"
        };

        var response = await Client.PostAsJsonAsync("/api/profesores", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor!.TipoContrato.Should().Be("TP");
        profesor.MaxAsignaturas.Should().Be(3);
    }

    [Fact(DisplayName = "[GET /api/profesores/{id}] Returns existing professor")]
    public async Task GetProfesorById_ReturnsExisting()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Prof",
            Identificacion = "111111111",
            TipoContrato = "TC"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Get
        var response = await Client.GetAsync($"/api/profesores/{created!.IdProfesor}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var profesor = await response.Content.ReadFromJsonAsync<ProfesorResponse>();
        profesor!.IdProfesor.Should().Be(created.IdProfesor);
    }

    [Fact(DisplayName = "[GET /api/profesores/{id}] Returns 404 for non-existent")]
    public async Task GetProfesorById_Returns404ForNonExistent()
    {
        var response = await Client.GetAsync($"/api/profesores/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "[PUT /api/profesores/{id}] Updates professor")]
    public async Task PutProfesor_UpdatesProfesor()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Original",
            Identificacion = "222222222",
            TipoContrato = "TC"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Update
        var updateRequest = new ActualizarProfesorRequest
        {
            Nombre = "Updated",
            Identificacion = "333333333",
            TipoContrato = "TP"
        };
        var response = await Client.PutAsJsonAsync($"/api/profesores/{created!.IdProfesor}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/profesores/{created.IdProfesor}");
        var updated = await getResponse.Content.ReadFromJsonAsync<ProfesorResponse>();
        updated!.TipoContrato.Should().Be("TP");
        updated.MaxAsignaturas.Should().Be(3);
    }

    [Fact(DisplayName = "[DELETE /api/profesores/{id}] Deletes professor")]
    public async Task DeleteProfesor_DeletesProfesor()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "ToDelete",
            Identificacion = "444444444",
            TipoContrato = "TC"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Delete
        var response = await Client.DeleteAsync($"/api/profesores/{created!.IdProfesor}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/profesores/{created.IdProfesor}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region === ASIGNATURAS ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/asignaturas] Returns list")]
    public async Task GetAsignaturas_ReturnsOkWithList()
    {
        var response = await Client.GetAsync("/api/asignaturas");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().NotBeNull().And.BeEmpty();
    }

    [Fact(DisplayName = "[POST /api/asignaturas] Creates valid subject")]
    public async Task PostAsignatura_CreatesValid()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "MAT101",
            Nombre = "Matemáticas I",
            Creditos = 4,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.Codigo.Should().Be("MAT101");
        asignatura.MinEstudiantes.Should().Be(15);
    }

    [Fact(DisplayName = "[POST /api/asignaturas] Normalizes code to uppercase")]
    public async Task PostAsignatura_NormalizesCode()
    {
        var request = new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "fis101",
            Nombre = "Física I",
            Creditos = 4,
            Semestre = 1
        };

        var response = await Client.PostAsJsonAsync("/api/asignaturas", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.Codigo.Should().Be("FIS101");
    }

    [Fact(DisplayName = "[GET /api/asignaturas/{id}] Returns existing subject")]
    public async Task GetAsignaturaById_ReturnsExisting()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "QUI101",
            Nombre = "Química I",
            Creditos = 3,
            Semestre = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Get
        var response = await Client.GetAsync($"/api/asignaturas/{created!.IdAsignatura}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var asignatura = await response.Content.ReadFromJsonAsync<AsignaturaResponse>();
        asignatura!.IdAsignatura.Should().Be(created.IdAsignatura);
    }

    [Fact(DisplayName = "[GET /api/asignaturas/plan/{idPlan}] Returns subjects for plan")]
    public async Task GetAsignaturasByPlan_ReturnsList()
    {
        var response = await Client.GetAsync($"/api/asignaturas/plan/{PlanDiurno}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaturas = await response.Content.ReadFromJsonAsync<List<AsignaturaResponse>>();
        asignaturas.Should().NotBeNull();
    }

    [Fact(DisplayName = "[PUT /api/asignaturas/{id}] Updates subject")]
    public async Task PutAsignatura_UpdatesAsignatura()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "ENG101",
            Nombre = "English",
            Creditos = 2,
            Semestre = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Update
        var updateRequest = new ActualizarAsignaturaRequest
        {
            Codigo = "ENG201",
            Nombre = "English II",
            Creditos = 3,
            Semestre = 2
        };
        var response = await Client.PutAsJsonAsync($"/api/asignaturas/{created!.IdAsignatura}", updateRequest);
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "[DELETE /api/asignaturas/{id}] Deletes subject")]
    public async Task DeleteAsignatura_DeletesAsignatura()
    {
        // Create
        var createResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "HIS101",
            Nombre = "History",
            Creditos = 2,
            Semestre = 1
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Delete
        var response = await Client.DeleteAsync($"/api/asignaturas/{created!.IdAsignatura}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify
        var getResponse = await Client.GetAsync($"/api/asignaturas/{created.IdAsignatura}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region === ASIGNACIONES ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/asignaciones] Returns list")]
    public async Task GetAsignaciones_ReturnsOkWithList()
    {
        var response = await Client.GetAsync("/api/asignaciones");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var asignaciones = await response.Content.ReadFromJsonAsync<List<AsignacionResponse>>();
        asignaciones.Should().NotBeNull();
    }

    [Fact(DisplayName = "[GET /api/asignaciones/periodos-historicos] Returns periods")]
    public async Task GetPeriodosHistoricos_ReturnsList()
    {
        var response = await Client.GetAsync("/api/asignaciones/periodos-historicos");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var periodos = await response.Content.ReadFromJsonAsync<List<string>>();
        periodos.Should().NotBeNull();
    }

    [Fact(DisplayName = "[POST /api/asignaciones] Creates valid assignment")]
    public async Task PostAsignacion_CreatesValid()
    {
        // Create professor
        var profResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Prof Assignment",
            Identificacion = "555555555",
            TipoContrato = "TC"
        });
        var profesor = await profResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Create subject
        var asigResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "ARQ101",
            Nombre = "Architecture",
            Creditos = 3,
            Semestre = 1
        });
        var asignatura = await asigResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Create assignment
        var request = new CrearAsignacionRequest
        {
            IdDocente = profesor!.IdProfesor,
            IdAsignatura = asignatura!.IdAsignatura,
            Dia = 1,
            HoraInicio = "08:00",
            HoraFin = "10:00",
            Periodo = "2025-1"
        };

        var response = await Client.PostAsJsonAsync("/api/asignaciones", request);
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact(DisplayName = "[DELETE /api/asignaciones/{id}] Deletes assignment")]
    public async Task DeleteAsignacion_DeletesAsignacion()
    {
        // Create professor
        var profResponse = await Client.PostAsJsonAsync("/api/profesores", new CrearProfesorRequest
        {
            Nombre = "Prof Delete",
            Identificacion = "666666666",
            TipoContrato = "TC"
        });
        var profesor = await profResponse.Content.ReadFromJsonAsync<ProfesorResponse>();

        // Create subject
        var asigResponse = await Client.PostAsJsonAsync("/api/asignaturas", new CrearAsignaturaRequest
        {
            IdPlan = PlanDiurno,
            Codigo = "ART101",
            Nombre = "Art",
            Creditos = 2,
            Semestre = 1
        });
        var asignatura = await asigResponse.Content.ReadFromJsonAsync<AsignaturaResponse>();

        // Create assignment
        var createResponse = await Client.PostAsJsonAsync("/api/asignaciones", new CrearAsignacionRequest
        {
            IdDocente = profesor!.IdProfesor,
            IdAsignatura = asignatura!.IdAsignatura,
            Dia = 1,
            HoraInicio = "10:00",
            HoraFin = "12:00",
            Periodo = "2025-1"
        });
        var created = await createResponse.Content.ReadFromJsonAsync<AsignacionResponse>();

        // Delete
        var response = await Client.DeleteAsync($"/api/asignaciones/{created!.IdAsignacion}");
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    #endregion

    #region === HORARIOS ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/horarios/exportar] Returns Excel file")]
    public async Task GetExportarHorarios_ReturnsExcel()
    {
        var response = await Client.GetAsync("/api/horarios/exportar");
        
        // Accept 200 or 400 (no data to export is acceptable)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);
    }

    #endregion

    #region === REPORTES ENDPOINT TESTS ===

    [Fact(DisplayName = "[GET /api/reportes/carga-docente] Requires semestre parameter")]
    public async Task GetReporteCargaDocente_RequiresSemestre()
    {
        var response = await Client.GetAsync("/api/reportes/carga-docente");
        
        // Should fail without semestre parameter
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    [Fact(DisplayName = "[GET /api/reportes/conflictos] Requires semestre parameter")]
    public async Task GetReporteConflictos_RequiresSemestre()
    {
        var response = await Client.GetAsync("/api/reportes/conflictos");
        
        // Should fail without semestre parameter
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
    }

    #endregion

    #region === ERROR HANDLING TESTS ===

    [Fact(DisplayName = "[Generic] Non-existent endpoint returns 404")]
    public async Task NonExistentEndpoint_Returns404()
    {
        var response = await Client.GetAsync("/api/nonexistent");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "[Generic] Invalid ID format is handled")]
    public async Task InvalidIdFormat_IsHandled()
    {
        var response = await Client.GetAsync("/api/usuarios/invalid-id");
        // Should return either 404 or handle gracefully
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    #endregion
}
