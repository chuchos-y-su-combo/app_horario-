using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// Factoría de aplicaciones web usada en pruebas de integración.
/// Configura una base de datos en memoria y autenticación de prueba.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    /// <summary>
    /// Crea una nueva factoría con un nombre de base de datos único por ejecución.
    /// </summary>
    public CustomWebApplicationFactory()
    {
        _databaseName = $"TestDb_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
    }

    /// <summary>
    /// Configura el host de pruebas para reemplazar la base de datos real por InMemory.
    /// </summary>
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

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                _ => { }
            );
        });
    }

    /// <summary>
    /// Inicializa la base de datos en memoria con los catálogos mínimos requeridos por los tests.
    /// </summary>
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

    /// <summary>
    /// Ejecuta una acción sobre un <see cref="AppDbContext"/> temporal de pruebas.
    /// </summary>
    public async Task ExecuteDbContextAsync(Func<AppDbContext, Task> action)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        await using var dbContext = new AppDbContext(options);
        await action(dbContext);
    }

    /// <summary>
    /// Ejecuta una función sobre un <see cref="AppDbContext"/> temporal de pruebas y devuelve un resultado.
    /// </summary>
    public async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> action)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;

        await using var dbContext = new AppDbContext(options);
        return await action(dbContext);
    }

    /// <summary>
    /// Limpia el estado mutable de la base de datos entre tests.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await ExecuteDbContextAsync(async dbContext =>
        {

            var bloqueos = await dbContext.BloqueosFranjaAsignatura.ToListAsync();
            dbContext.BloqueosFranjaAsignatura.RemoveRange(bloqueos);
            
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