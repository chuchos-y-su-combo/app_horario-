using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Usuarios;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

public class UsuariosControllerTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public UsuariosControllerTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "GET /api/usuarios - Retorna lista vacía inicialmente")]
    public async Task ObtenerTodos_RetornaListaVacia()
    {
        var response = await Client.GetAsync("/api/usuarios");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuarios = await response.Content.ReadFromJsonAsync<List<UsuarioResponse>>();
        usuarios.Should().NotBeNull();
        usuarios.Should().BeEmpty();
    }

    [Fact(DisplayName = "REQ 2 - Crea usuario Administrador")]
    public async Task Crear_UsuarioAdministrador()
    {
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Admin",
            Correo = "ADMIN@TEST.COM",
            Password = "Password123",
            IdRol = 1
        };

        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario.Should().NotBeNull();
        usuario!.Correo.Should().Be("admin@test.com");
        usuario.IdRol.Should().Be(1);
        usuario.NombreRol.Should().Be("Administrador");
    }

    [Fact(DisplayName = "REQ 2 - Crea usuario Coordinador")]
    public async Task Crear_UsuarioCoordinador()
    {
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Coordinador",
            Correo = "coordinador@test.com",
            Password = "Password123",
            IdRol = 2
        };

        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario!.IdRol.Should().Be(2);
        usuario.NombreRol.Should().Be("Coordinador");
    }

    [Fact(DisplayName = "POST /api/usuarios - Rechaza correo duplicado")]
    public async Task Crear_RechazaCorreoDuplicado()
    {
        var request1 = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Uno",
            Correo = "duplicado@test.com",
            Password = "Password123",
            IdRol = 1
        };

        var request2 = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Dos",
            Correo = "DUPLICADO@TEST.COM",
            Password = "Password123",
            IdRol = 2
        };

        await Client.PostAsJsonAsync("/api/usuarios", request1);

        var response = await Client.PostAsJsonAsync("/api/usuarios", request2);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("correo");
    }

    [Fact(DisplayName = "POST /api/usuarios - Rechaza rol inexistente")]
    public async Task Crear_RechazaRolInexistente()
    {
        var request = new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Sin Rol",
            Correo = "sinrol@test.com",
            Password = "Password123",
            IdRol = 999
        };

        var response = await Client.PostAsJsonAsync("/api/usuarios", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("rol");
    }

    [Fact(DisplayName = "GET /api/usuarios/{id} - Obtiene usuario existente")]
    public async Task ObtenerPorId_RetornaUsuario()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Consulta",
            Correo = "consulta@test.com",
            Password = "Password123",
            IdRol = 1
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await Client.GetAsync($"/api/usuarios/{creado!.IdUsuario}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();
        usuario!.IdUsuario.Should().Be(creado.IdUsuario);
        usuario.Correo.Should().Be("consulta@test.com");
    }

    [Fact(DisplayName = "PUT /api/usuarios/{id} - Actualiza usuario")]
    public async Task Actualizar_Usuario()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Original",
            Correo = "original@test.com",
            Password = "Password123",
            IdRol = 1
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        var actualizarRequest = new ActualizarUsuarioRequest
        {
            NombreCompleto = "Usuario Actualizado",
            Correo = "actualizado@test.com",
            IdRol = 2
        };

        var response = await Client.PutAsJsonAsync($"/api/usuarios/{creado!.IdUsuario}", actualizarRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/usuarios/{creado.IdUsuario}");
        var actualizado = await getResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        actualizado!.NombreCompleto.Should().Be("Usuario Actualizado");
        actualizado.Correo.Should().Be("actualizado@test.com");
        actualizado.IdRol.Should().Be(2);
        actualizado.NombreRol.Should().Be("Coordinador");
    }

    [Fact(DisplayName = "PATCH /api/usuarios/{id}/password - Cambia contraseña")]
    public async Task CambiarPassword_UsuarioExistente()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Password",
            Correo = "password@test.com",
            Password = "Password123",
            IdRol = 1
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await Client.PatchAsJsonAsync($"/api/usuarios/{creado!.IdUsuario}/password", new CambiarPasswordRequest
        {
            NuevaPassword = "NuevaPassword123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact(DisplayName = "DELETE /api/usuarios/{id} - Elimina usuario")]
    public async Task Eliminar_Usuario()
    {
        var crearResponse = await Client.PostAsJsonAsync("/api/usuarios", new CrearUsuarioRequest
        {
            NombreCompleto = "Usuario Eliminable",
            Correo = "eliminar@test.com",
            Password = "Password123",
            IdRol = 1
        });

        var creado = await crearResponse.Content.ReadFromJsonAsync<UsuarioResponse>();

        var response = await Client.DeleteAsync($"/api/usuarios/{creado!.IdUsuario}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/usuarios/{creado.IdUsuario}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}