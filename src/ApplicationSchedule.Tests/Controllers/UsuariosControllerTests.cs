using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

/// <summary>
/// Suite completa de pruebas para los 6 endpoints de Usuarios API.
/// Todas las pruebas son independientes y siguen el patrón AAA (Arrange/Act/Assert).
/// </summary>
public class UsuariosControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public UsuariosControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region GET /api/usuarios

    [Fact(DisplayName = "GET /api/usuarios - Retorna lista vacía cuando no hay usuarios")]
    public async Task ObtenerTodos_RetornListaVacia_CuandoNoHayUsuarios()
    {
        // Arrange: BD limpia con solo roles

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuarios.Should().NotBeNull();
        usuarios.Should().BeEmpty();
    }

    [Fact(DisplayName = "GET /api/usuarios - Retorna lista ordenada de usuarios")]
    public async Task ObtenerTodos_RetornListaOrdenada_CuandoHayMultiplesUsuarios()
    {
        // Arrange: Crear 3 usuarios con nombres diferentes
        var usuariosACrear = new[]
        {
            new CrearUsuarioRequest
            {
                NombreCompleto = "Zara López",
                Correo = $"zara_{Guid.NewGuid():N}@test.com",
                Password = "SecurePass123!",
                IdRol = 1
            },
            new CrearUsuarioRequest
            {
                NombreCompleto = "Admin User",
                Correo = $"admin_{Guid.NewGuid():N}@test.com",
                Password = "SecurePass123!",
                IdRol = 1
            },
            new CrearUsuarioRequest
            {
                NombreCompleto = "María García",
                Correo = $"maria_{Guid.NewGuid():N}@test.com",
                Password = "SecurePass123!",
                IdRol = 2
            }
        };

        foreach (var request in usuariosACrear)
        {
            await Client.PostAsJsonAsync("/api/usuarios", request);
        }

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuarios.Should().NotBeNull();
        usuarios.Should().HaveCount(3);
        // Verificar orden alfabético por nombre completo
        usuarios.Should().BeInAscendingOrder(u => u.NombreCompleto);
        usuarios![0].NombreCompleto.Should().Be("Admin User");
        usuarios[1].NombreCompleto.Should().Be("María García");
        usuarios[2].NombreCompleto.Should().Be("Zara López");
    }

    #endregion

    #region GET /api/usuarios/{idUsuario}

    [Fact(DisplayName = "GET /api/usuarios/{id} - Retorna usuario existente")]
    public async Task ObtenerPorId_RetornUsuario_CuandoIdExiste()
    {
        // Arrange: Crear un usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Juan Pérez",
            Correo = $"juan_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Act
        var getResponse = await Client.GetAsync($"/api/usuarios/{usuarioCreado!.IdUsuario}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarioObtenido = await getResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioObtenido.Should().NotBeNull();
        usuarioObtenido!.IdUsuario.Should().Be(usuarioCreado.IdUsuario);
        usuarioObtenido.NombreCompleto.Should().Be(crearRequest.NombreCompleto);
        usuarioObtenido.Correo.Should().Be(crearRequest.Correo.ToLower());
        usuarioObtenido.IdRol.Should().Be(1);
        usuarioObtenido.NombreRol.Should().Be("Administrador");
    }

    [Fact(DisplayName = "GET /api/usuarios/{id} - Retorna 404 cuando usuario no existe")]
    public async Task ObtenerPorId_Retorn404_CuandoIdNoExiste()
    {
        // Arrange: ID que no existe (GUID cualquiera)
        var idNoExistente = Guid.NewGuid().ToString();

        // Act
        var response = await Client.GetAsync($"/api/usuarios/{idNoExistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/usuarios

    [Fact(DisplayName = "POST /api/usuarios - Crea usuario válido exitosamente")]
    public async Task Crear_RetornCreatedAt_ConDatosValidos()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Nuevo Usuario",
            Correo = $"nuevo_{Guid.NewGuid():N}@example.com",
            Password = "SecurePass123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var usuarioCreado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioCreado.Should().NotBeNull();
        usuarioCreado!.IdUsuario.Should().NotBeEmpty();
        usuarioCreado.NombreCompleto.Should().Be(request.NombreCompleto);
        usuarioCreado.Correo.Should().Be(request.Correo.ToLower());
        usuarioCreado.IdRol.Should().Be(1);
    }

    [Fact(DisplayName = "POST /api/usuarios - Rechaza correo duplicado")]
    public async Task Crear_RetornBadRequest_CuandoCorreoDuplicado()
    {
        // Arrange: Crear primer usuario
        var correo = $"usuario_{Guid.NewGuid():N}@test.com";
        var request1 = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 1",
            Correo = correo,
            Password = "SecurePass123!",
            IdRol = 1
        };
        await Client.PostAsJsonAsync("/api/usuarios", request1);

        // Intentar crear segundo usuario con mismo correo
        var request2 = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 2",
            Correo = correo,
            Password = "OtherPass123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request2);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.ReadAsStringAsync().Result.Should().Contain("correo");
    }

    [Fact(DisplayName = "POST /api/usuarios - Rechaza rol inexistente")]
    public async Task Crear_RetornBadRequest_CuandoRolNoExiste()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Test",
            Correo = $"test_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 999 // Rol que no existe
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            response.Content.ReadAsStringAsync().Result.Should().Contain("rol");
    }

    [Theory(DisplayName = "POST /api/usuarios - Valida requerimientos de entrada")]
    [InlineData("", "test@test.com", "Pass123!", 1, "nombre")]
    [InlineData("User", "", "Pass123!", 1, "correo")]
    [InlineData("User", "invalid-email", "Pass123!", 1, "correo")]
    [InlineData("User", "test@test.com", "short", 1, "contraseña")]
    public async Task Crear_RetornBadRequest_ConValidacionesFallidas(
        string nombre, string correo, string password, int idRol, string fieldExpected)
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = nombre,
            Correo = correo,
            Password = password,
            IdRol = idRol
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact(DisplayName = "POST /api/usuarios - Normaliza correo a minúsculas")]
    public async Task Crear_NormalizaCorreo_AMinusculas()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Test User",
            Correo = "TestUser@Example.COM",
            Password = "SecurePass123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);
        var usuarioCreado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Assert
        usuarioCreado!.Correo.Should().Be("testuser@example.com");
    }

    #endregion

    #region PUT /api/usuarios/{idUsuario}

    [Fact(DisplayName = "PUT /api/usuarios/{id} - Actualiza usuario exitosamente")]
    public async Task Actualizar_RetornNoContent_ConDatosValidos()
    {
        // Arrange: Crear usuario inicial
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Nombre Original",
            Correo = $"original_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuario = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Actualizar
        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Nombre Actualizado",
            Correo = $"actualizado_{Guid.NewGuid():N}@test.com",
            IdRol = 2
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{usuario!.IdUsuario}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar cambios
        var getResponse = await Client.GetAsync($"/api/usuarios/{usuario.IdUsuario}");
        var usuarioActualizado = await getResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioActualizado!.NombreCompleto.Should().Be(actualizarRequest.NombreCompleto);
        usuarioActualizado.Correo.Should().Be(actualizarRequest.Correo.ToLower());
        usuarioActualizado.IdRol.Should().Be(2);
    }

    [Fact(DisplayName = "PUT /api/usuarios/{id} - Retorna 404 cuando usuario no existe")]
    public async Task Actualizar_Retorn404_CuandoUsuarioNoExiste()
    {
        // Arrange
        var idNoExistente = Guid.NewGuid().ToString();
        var request = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Test",
            Correo = "test@test.com",
            IdRol = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{idNoExistente}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "PUT /api/usuarios/{id} - Rechaza correo duplicado")]
    public async Task Actualizar_RetornBadRequest_CuandoCorreoDuplicado()
    {
        // Arrange: Crear dos usuarios
        var user1Request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 1",
            Correo = $"user1_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };
        var user1Response = await Client.PostAsJsonAsync("/api/usuarios", user1Request);
        var user1 = await user1Response.Content.ReadFromJsonAsync<UsuarioResponse>();

        var user2Request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 2",
            Correo = $"user2_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };
        var user2Response = await Client.PostAsJsonAsync("/api/usuarios", user2Request);
        var user2 = await user2Response.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Intentar actualizar user1 con correo de user2
        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Usuario Modificado",
            Correo = user2Request.Correo,
            IdRol = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{user1!.IdUsuario}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PATCH /api/usuarios/{idUsuario}/password

    [Fact(DisplayName = "PATCH /api/usuarios/{id}/password - Cambia contraseña exitosamente")]
    public async Task CambiarPassword_RetornNoContent_ConContraseñaValida()
    {
        // Arrange: Crear usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario",
            Correo = $"user_{Guid.NewGuid():N}@test.com",
            Password = "OldPassword123!",
            IdRol = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuario = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        var cambiarPasswordRequest = new CambiarPasswordRequest
        {
            NuevaPassword = "NewPassword456!"
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"/api/usuarios/{usuario!.IdUsuario}/password",
            cambiarPasswordRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "PATCH /api/usuarios/{id}/password - Retorna 404 cuando usuario no existe")]
    public async Task CambiarPassword_Retorn404_CuandoUsuarioNoExiste()
    {
        // Arrange
        var idNoExistente = Guid.NewGuid().ToString();
        var request = new CambiarPasswordRequest { NuevaPassword = "NewPass123!" };

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/usuarios/{idNoExistente}/password", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/usuarios/{idUsuario}

    [Fact(DisplayName = "DELETE /api/usuarios/{id} - Elimina usuario exitosamente")]
    public async Task Eliminar_RetornNoContent_CuandoUsuarioExiste()
    {
        // Arrange: Crear usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario a Eliminar",
            Correo = $"eliminar_{Guid.NewGuid():N}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuario = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Act
        var response = await Client.DeleteAsync($"/api/usuarios/{usuario!.IdUsuario}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que no existe
        var getResponse = await Client.GetAsync($"/api/usuarios/{usuario.IdUsuario}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact(DisplayName = "DELETE /api/usuarios/{id} - Retorna 404 cuando usuario no existe")]
    public async Task Eliminar_Retorn404_CuandoUsuarioNoExiste()
    {
        // Arrange
        var idNoExistente = Guid.NewGuid().ToString();

        // Act
        var response = await Client.DeleteAsync($"/api/usuarios/{idNoExistente}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
