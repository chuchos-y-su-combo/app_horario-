using System.Net;
using System.Net.Http.Json;
using ApplicationSchedule.Application.DTOs.Auth;
using ApplicationSchedule.Application.Security;
using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Xunit;

namespace ApplicationSchedule.Tests.Controllers;

public class AutorizacionPorRolTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public AutorizacionPorRolTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "ROL - Administrador puede acceder a gestión de usuarios")]
    public async Task Administrador_PuedeAcceder_AUsuarios()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/usuarios");
        request.Headers.Add("X-Test-Role", RolesSistema.Administrador);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "ROL - Coordinador no puede acceder a gestión de usuarios")]
    public async Task Coordinador_NoPuedeAcceder_AUsuarios()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/usuarios");
        request.Headers.Add("X-Test-Role", RolesSistema.Coordinador);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact(DisplayName = "ROL - Usuario sin autenticación no puede acceder a gestión de usuarios")]
    public async Task SinAutenticacion_NoPuedeAcceder_AUsuarios()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/usuarios");
        request.Headers.Add("X-Test-Anonymous", "true");

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact(DisplayName = "ROL - Coordinador puede acceder a módulos operativos")]
    public async Task Coordinador_PuedeAcceder_AProfesores()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/profesores");
        request.Headers.Add("X-Test-Role", RolesSistema.Coordinador);

        var response = await Client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(DisplayName = "AUTH - Login devuelve el rol del usuario autenticado")]
    public async Task Login_DevuelveRolDelUsuario()
    {
        await Factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Usuarios.Add(new Usuario
            {
                IdUsuario = Guid.NewGuid().ToString(),
                NombreCompleto = "Administrador Prueba",
                Correo = "admin.roles@test.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123"),
                IdRol = 1
            });

            await dbContext.SaveChangesAsync();
        });

        var response = await Client.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Correo = "admin.roles@test.com",
            Password = "Password123"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        LoginResponse login = (await response.Content.ReadFromJsonAsync<LoginResponse>())!;

        login.Token.Should().NotBeNullOrWhiteSpace();
        login.Rol.Should().Be(RolesSistema.Administrador);
        login.Correo.Should().Be("admin.roles@test.com");
    }
}