using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using ApplicationSchedule.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// Pruebas de integración que validan el modelo de datos y la configuración del contexto.
/// </summary>
public class DatabaseModelTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
{
    public DatabaseModelTests(CustomWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact(DisplayName = "BD oficial SQLite - Usa nombres de tablas oficiales")]
    public async Task Modelo_UsaTablasOficiales()
    {
        await Factory.ExecuteDbContextAsync(async dbContext =>
        {
            dbContext.Model.FindEntityType(typeof(BloqueoFranjaAsignatura))!.GetTableName().Should().Be("bloqueos_franja_asignatura");
            dbContext.Model.FindEntityType(typeof(Rol))!.GetTableName().Should().Be("roles");
            dbContext.Model.FindEntityType(typeof(Usuario))!.GetTableName().Should().Be("usuarios");
            dbContext.Model.FindEntityType(typeof(PlanEstudio))!.GetTableName().Should().Be("planes_estudio");
            dbContext.Model.FindEntityType(typeof(Docente))!.GetTableName().Should().Be("docentes");
            dbContext.Model.FindEntityType(typeof(Asignatura))!.GetTableName().Should().Be("asignaturas");
            dbContext.Model.FindEntityType(typeof(Asignacion))!.GetTableName().Should().Be("asignaciones");

            await Task.CompletedTask;
        });
    }

    [Fact(DisplayName = "BD oficial SQLite - Tiene roles base y planes base")]
    public async Task Modelo_TieneDatosBase()
    {
        await Factory.ExecuteDbContextAsync(async dbContext =>
        {
            var roles = await dbContext.Roles.OrderBy(r => r.IdRol).ToListAsync();
            roles.Should().HaveCount(2);
            roles[0].NombreRol.Should().Be("Administrador");
            roles[1].NombreRol.Should().Be("Coordinador");

            var planes = await dbContext.PlanesEstudio.OrderBy(p => p.Jornada).ToListAsync();
            planes.Should().HaveCount(2);
            planes.Should().Contain(p => p.Jornada == "Diurna");
            planes.Should().Contain(p => p.Jornada == "Nocturna");
        });
    }
}