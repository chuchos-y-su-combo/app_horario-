using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Usuarios;

/// <summary>
/// Suite completa de pruebas para los endpoints de la API de Usuarios.
/// Todos los tests son independientes y no dependen de datos previos.
/// Sigue el patrón Arrange / Act / Assert (AAA).
/// </summary>
public class UsuariosApiTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public UsuariosApiTests(CustomWebApplicationFactory factory) : base(factory) { }

    #region GET /api/usuarios

    [Fact]
    public async Task ObtenerTodos_RetornListaVacia_CuandoNoHayUsuarios()
    {
        // Arrange: No hay datos iniciales, la BD está limpia

        // Act
        var response = await Client.GetAsync("/api/usuarios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuarios.Should().NotBeNull();
        usuarios.Should().BeEmpty();
    }

    [Fact]
    public async Task ObtenerTodos_RetornListaUsuarios_CuandoHayMultiplesUsuarios()
    {
        // Arrange: Crear 3 usuarios diferentes con datos dinámicos
        var usuariosCreados = new List<UsuarioResponse>();
        for (int i = 0; i < 3; i++)
        {
            var request = new CrearUsuarioRequest
            {
                NombreCompleto = $"Usuario Test {i}",
                Correo = $"usuario{i}_{Guid.NewGuid()}@test.com",
                Password = "Password123!",
                IdRol = 1 // Administrador
            };

            var response = await Client.PostAsJsonAsync("/api/usuarios", request);
            var usuarioCreado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
            usuariosCreados.Add(usuarioCreado!);
        }

        // Act
        var getResponse = await Client.GetAsync("/api/usuarios");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuariosObtenidos = await getResponse.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuariosObtenidos.Should().NotBeNull();
        usuariosObtenidos.Should().HaveCount(3);
        usuariosObtenidos.Should().BeInAscendingOrder(u => u.NombreCompleto);
    }

    #endregion

    #region GET /api/usuarios/{id}

    [Fact]
    public async Task ObtenerPorId_RetornUsuario_CuandoIdExiste()
    {
        // Arrange: Crear un usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Juan Pérez",
            Correo = $"juan_{Guid.NewGuid()}@test.com",
            Password = "SecurePass123!",
            IdRol = 1
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        var idUsuario = usuarioCreado!.IdUsuario;

        // Act
        var response = await Client.GetAsync($"/api/usuarios/{idUsuario}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarioObtenido = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioObtenido.Should().NotBeNull();
        usuarioObtenido!.IdUsuario.Should().Be(idUsuario);
        usuarioObtenido.NombreCompleto.Should().Be(crearRequest.NombreCompleto);
        usuarioObtenido.Correo.Should().Be(crearRequest.Correo.ToLower());
    }

    [Fact]
    public async Task ObtenerPorId_Retorn404_CuandoIdNoExiste()
    {
        // Arrange: Usar un ID que no existe (GUID válido pero no en BD)
        var idInvalido = Guid.NewGuid().ToString();

        // Act
        var response = await Client.GetAsync($"/api/usuarios/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        json.TryGetProperty("mensaje", out var mensajeProp).Should().BeTrue();
        mensajeProp.GetString().Should().Contain("Usuario no encontrado");
    }

    #endregion

    #region POST /api/usuarios - Crear

    [Fact]
    public async Task Crear_Retorn201_CuandoDatosValidos()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "María García",
            Correo = $"maria_{Guid.NewGuid()}@test.com",
            Password = "StrongPass123!",
            IdRol = 2 // Coordinador
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var usuarioCreado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioCreado.Should().NotBeNull();
        usuarioCreado!.IdUsuario.Should().NotBeEmpty();
        usuarioCreado.NombreCompleto.Should().Be(request.NombreCompleto.Trim());
        usuarioCreado.Correo.Should().Be(request.Correo.ToLower());
        usuarioCreado.IdRol.Should().Be(request.IdRol);
        usuarioCreado.NombreRol.Should().Be("Coordinador");
    }

    [Fact]
    public async Task Crear_Retorn400_CuandoCorreoDuplicado()
    {
        // Arrange: Crear el primer usuario
        var correoRepetido = $"repetido_{Guid.NewGuid()}@test.com";
        var primerRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Uno",
            Correo = correoRepetido,
            Password = "Password123!",
            IdRol = 1
        };

        await Client.PostAsJsonAsync("/api/usuarios", primerRequest);

        // Intentar crear otro con el mismo correo
        var segundoRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Dos",
            Correo = correoRepetido,
            Password = "Password123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", segundoRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        json.TryGetProperty("mensaje", out var mensajeProp).Should().BeTrue();
        mensajeProp.GetString().Should().Contain("Ya existe");
    }

    [Fact]
    public async Task Crear_Retorn400_CuandoRolNoExiste()
    {
        // Arrange: Usar un IdRol que no existe
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Test",
            Correo = $"test_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 999 // Rol inexistente
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        json.TryGetProperty("mensaje", out var mensajeProp).Should().BeTrue();
        mensajeProp.GetString().Should().Contain("no existe");
    }

    [Fact]
    public async Task Crear_Retorn400_CuandoNombreVacio()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = string.Empty, // Inválido
            Correo = $"test_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Crear_Retorn400_CuandoCorreoFormatoInvalido()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Test",
            Correo = "correo_invalido", // No es email válido
            Password = "Password123!",
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Crear_Retorn400_CuandoPasswordMenor8Caracteres()
    {
        // Arrange
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Test",
            Correo = $"test_{Guid.NewGuid()}@test.com",
            Password = "Pass12", // Menos de 8 caracteres
            IdRol = 1
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PUT /api/usuarios/{id} - Actualizar

    [Fact]
    public async Task Actualizar_Retorn204_CuandoDatosValidos()
    {
        // Arrange: Crear un usuario primero
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Original",
            Correo = $"original_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 1
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        var idUsuario = usuarioCreado!.IdUsuario;

        // Preparar datos para actualización
        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Usuario Actualizado",
            Correo = $"actualizado_{Guid.NewGuid()}@test.com",
            IdRol = 2
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{idUsuario}", actualizarRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que realmente se actualizó
        var getResponse = await Client.GetAsync($"/api/usuarios/{idUsuario}");
        var usuarioActualizado = await getResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioActualizado!.NombreCompleto.Should().Be(actualizarRequest.NombreCompleto);
        usuarioActualizado.Correo.Should().Be(actualizarRequest.Correo.ToLower());
        usuarioActualizado.IdRol.Should().Be(2);
    }

    [Fact]
    public async Task Actualizar_Retorn404_CuandoIdNoExiste()
    {
        // Arrange
        var idInvalido = Guid.NewGuid().ToString();
        var request = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Nuevo Nombre",
            Correo = $"nuevo_{Guid.NewGuid()}@test.com",
            IdRol = 1
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/usuarios/{idInvalido}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Actualizar_Retorn400_CuandoCorreoDuplicado()
    {
        // Arrange: Crear dos usuarios
        var usuario1Request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 1",
            Correo = $"usuario1_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 1
        };

        var usuario2Request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario 2",
            Correo = $"usuario2_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 1
        };

        var response1 = await Client.PostAsJsonAsync("/api/usuarios", usuario1Request);
        var usuario1 = await response1.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response2 = await Client.PostAsJsonAsync("/api/usuarios", usuario2Request);
        var usuario2 = await response2.Content.ReadFromJsonAsync<UsuarioResponse>();

        // Intentar actualizar usuario 2 con el correo del usuario 1
        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Usuario 2 Actualizado",
            Correo = usuario1Request.Correo, // Correo duplicado
            IdRol = 1
        };

        // Act
        var updateResponse = await Client.PutAsJsonAsync($"/api/usuarios/{usuario2!.IdUsuario}", actualizarRequest);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region PATCH /api/usuarios/{id}/password - Cambiar Contraseña

    [Fact]
    public async Task CambiarPassword_Retorn204_CuandoDatosValidos()
    {
        // Arrange: Crear un usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Password",
            Correo = $"password_{Guid.NewGuid()}@test.com",
            Password = "OldPassword123!",
            IdRol = 1
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        var idUsuario = usuarioCreado!.IdUsuario;

        var cambiarPasswordRequest = new CambiarPasswordRequest
        {
            NuevaPassword = "NewPassword456!"
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"/api/usuarios/{idUsuario}/password",
            cambiarPasswordRequest
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        // Nota: No podemos verificar el hash directamente, pero el cambio fue exitoso
    }

    [Fact]
    public async Task CambiarPassword_Retorn404_CuandoIdNoExiste()
    {
        // Arrange
        var idInvalido = Guid.NewGuid().ToString();
        var request = new CambiarPasswordRequest
        {
            NuevaPassword = "NewPassword123!"
        };

        // Act
        var response = await Client.PatchAsJsonAsync(
            $"/api/usuarios/{idInvalido}/password",
            request
        );

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/usuarios/{id} - Eliminar

    [Fact]
    public async Task Eliminar_Retorn204_CuandoIdExiste()
    {
        // Arrange: Crear un usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Para Eliminar",
            Correo = $"eliminar_{Guid.NewGuid()}@test.com",
            Password = "Password123!",
            IdRol = 1
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        var idUsuario = usuarioCreado!.IdUsuario;

        // Act
        var deleteResponse = await Client.DeleteAsync($"/api/usuarios/{idUsuario}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que realmente se eliminó
        var getResponse = await Client.GetAsync($"/api/usuarios/{idUsuario}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Eliminar_Retorn404_CuandoIdNoExiste()
    {
        // Arrange
        var idInvalido = Guid.NewGuid().ToString();

        // Act
        var response = await Client.DeleteAsync($"/api/usuarios/{idInvalido}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Tests de Integración Completos

    [Fact]
    public async Task FlujoCOMPLETO_CrearActualizarEliminarUsuario()
    {
        // Este test verifica el flujo completo de un usuario: crear -> leer -> actualizar -> eliminar

        // CREAR usuario
        var crearRequest = new CrearUsuarioRequest
        {
            NombreCompleto = "Flujo Completo Test",
            Correo = $"flujo_{Guid.NewGuid()}@test.com",
            Password = "FlowPassword123!",
            IdRol = 1
        };

        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", crearRequest);
        crearResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var usuarioCreado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        var idUsuario = usuarioCreado!.IdUsuario;

        // OBTENER usuario creado
        var obtenerResponse = await Client.GetAsync($"/api/usuarios/{idUsuario}");
        obtenerResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var usuarioObtenido = await obtenerResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioObtenido!.Correo.Should().Be(crearRequest.Correo.ToLower());

        // ACTUALIZAR usuario
        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Nombre Actualizado en Flujo",
            Correo = $"flujo_actualizado_{Guid.NewGuid()}@test.com",
            IdRol = 2
        };

        var actualizarResponse = await Client.PutAsJsonAsync($"/api/usuarios/{idUsuario}", actualizarRequest);
        actualizarResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar actualización
        var verificarResponse = await Client.GetAsync($"/api/usuarios/{idUsuario}");
        var usuarioActualizado = await verificarResponse.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuarioActualizado!.NombreCompleto.Should().Be(actualizarRequest.NombreCompleto);

        // CAMBIAR PASSWORD
        var cambiarPasswordRequest = new CambiarPasswordRequest
        {
            NuevaPassword = "NewFlowPassword456!"
        };

        var passwordResponse = await Client.PatchAsJsonAsync($"/api/usuarios/{idUsuario}/password", cambiarPasswordRequest);
        passwordResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // ELIMINAR usuario
        var eliminarResponse = await Client.DeleteAsync($"/api/usuarios/{idUsuario}");
        eliminarResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar que fue eliminado
        var verificarEliminacionResponse = await Client.GetAsync($"/api/usuarios/{idUsuario}");
        verificarEliminacionResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}

