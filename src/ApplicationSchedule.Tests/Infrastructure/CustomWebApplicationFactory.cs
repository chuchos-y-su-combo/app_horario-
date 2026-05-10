using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationSchedule.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", string.Empty }
            };

            config.AddInMemoryCollection(dict);
        });

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                d.ServiceType.FullName?.Contains("AppDbContext") == true
            ).ToList();

            foreach (var descriptor in descriptorsToRemove)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(
                options => options.UseInMemoryDatabase(_databaseName),
                ServiceLifetime.Transient
            );
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        await ExecuteDbContextAsync(async dbContext =>
        {
            await dbContext.Database.EnsureCreatedAsync();

            if (!await dbContext.Roles.AnyAsync())
            {
                dbContext.Roles.AddRange(
                    new Rol { IdRol = 1, NombreRol = "Administrador" },
                    new Rol { IdRol = 2, NombreRol = "Coordinador" }
                );
            }

            if (!await dbContext.PlanesEstudio.AnyAsync())
            {
                dbContext.PlanesEstudio.AddRange(
                    new PlanEstudio
                    {
                        IdPlan = "11111111-1111-1111-1111-111111111111",
                        NombrePlan = "Plan de Estudios 1020 Jornada Diurna",
                        Jornada = "Diurna"
                    },
                    new PlanEstudio
                    {
                        IdPlan = "22222222-2222-2222-2222-222222222222",
                        NombrePlan = "Plan de Estudios Jornada Noche",
                        Jornada = "Nocturna"
                    }
                );
            }

            await dbContext.SaveChangesAsync();
        });
    }

    public async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await action(dbContext);
    }

    public async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        await using var dbContext = new AppDbContext(options);
        return await action(dbContext);
    }

    public async Task ResetDatabaseAsync()
    {
        await ExecuteDbContextAsync(async dbContext =>
        {
            var asignaciones = await dbContext.Asignaciones.ToListAsync();
            dbContext.Asignaciones.RemoveRange(asignaciones);

            var usuarios = await dbContext.Usuarios.ToListAsync();
            dbContext.Usuarios.RemoveRange(usuarios);

            var asignaturas = await dbContext.Asignaturas.ToListAsync();
            dbContext.Asignaturas.RemoveRange(asignaturas);

            var docentes = await dbContext.Docentes.ToListAsync();
            dbContext.Docentes.RemoveRange(docentes);

            await dbContext.SaveChangesAsync();
        });
    }
}