using ApplicationSchedule.Domain.Entities;
using ApplicationSchedule.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// Factory personalizado para crear instancias de la aplicación con base de datos en memoria para pruebas.
/// ✅ AISLAMIENTO CRÍTICO:
/// - Cada instancia tiene su propia BD InMemory independiente (nombre único con GUID)
/// - Permite ejecución paralela de tests sin interferencias
/// - NO persiste datos entre test runs
/// - BD completamente limpia en cada initialization
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public CustomWebApplicationFactory()
    {
        // IMPORTANTE: Cada BD tiene nombre �nico. EF Core InMemory usa el nombre como key.
        // Dos tests NUNCA compartir�n BD incluso si se ejecutan en paralelo.
        _databaseName = $"TestDb_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Evitar que la app cargue la cadena de conexión de appsettings.json durante los tests.
        // Esto asegura que Program.cs detecte que no hay connection string y NO registre Swagger/OpenAPI.
        builder.ConfigureAppConfiguration((context, config) =>
        {
            var dict = new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", string.Empty }
            };
            config.AddInMemoryCollection(dict!);
        });

        // Ejecutar host en entorno "Testing" para que Program.cs pueda condicionar features (p.ej. Swagger)
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ✅ PASO 1: Remover cualquier registro relacionado con AppDbContext/DbContextOptions (elimina config de MySQL)
            var descriptorsToRemove = services.Where(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                d.ServiceType == typeof(AppDbContext) ||
                (d.ImplementationType != null && d.ImplementationType == typeof(AppDbContext)) ||
                (d.ServiceType?.FullName?.Contains("AppDbContext") == true)
            ).ToList();

            foreach (var d in descriptorsToRemove)
            {
                services.Remove(d);
            }
            
            // ✅ PASO 2: Registrar DbContext SOLO con InMemory
            services.AddDbContext<AppDbContext>(
                options => options.UseInMemoryDatabase(_databaseName),
                ServiceLifetime.Transient);

            // Remover servicios relacionados con Swagger/OpenAPI si fueron registrados
            var swaggerGen = services.FirstOrDefault(d => d.ServiceType?.FullName?.Contains("Swashbuckle") == true);
            if (swaggerGen != null)
            {
                services.Remove(swaggerGen);
            }

            // Agregar un IStartupFilter que limpie ApplicationParts problemáticos antes de que MVC haga el escaneo
            services.AddSingleton<Microsoft.AspNetCore.Hosting.IStartupFilter>(new RemoveSwaggerApplicationPartsStartupFilter());

            // Forzar que MVC use solo los ApplicationParts del ensamblado de la API (evita que se escaneen assemblies que referencian OpenAPI)
            var apiAssemblyName = typeof(Program).Assembly.GetName().Name;
            services.AddControllers().ConfigureApplicationPartManager(apm =>
            {
                var keep = apm.ApplicationParts.Where(p => p.Name == apiAssemblyName).ToList();
                apm.ApplicationParts.Clear();
                foreach (var p in keep)
                {
                    apm.ApplicationParts.Add(p);
                }
            });
        });
    }

    private class RemoveSwaggerApplicationPartsStartupFilter : Microsoft.AspNetCore.Hosting.IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                try
                {
                    var partManager = app.ApplicationServices.GetService<Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager>();
                    if (partManager != null)
                    {
                        var toRemove = partManager.ApplicationParts
                            .Where(p => p.Name?.Contains("Swashbuckle") == true || p.Name?.Contains("Microsoft.OpenApi") == true)
                            .ToList();

                        foreach (var p in toRemove)
                        {
                            partManager.ApplicationParts.Remove(p);
                        }
                    }
                }
                catch
                {
                    // No fallar en el startup filter; en worst-case MVC hará su trabajo normalmente
                }

                next(app);
            };
        }
    }

    /// <summary>
    /// Inicializa la BD: crea esquema y siembra roles base.
    /// Se llama DESPUÉS de que el host esté completamente listo.
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        await ExecuteDbContextAsync(async dbContext =>
        {
            await dbContext.Database.EnsureCreatedAsync();
            
            if (!await dbContext.Roles.AnyAsync())
            {
                var roles = new[]
                {
                    new Rol { IdRol = 1, NombreRol = "Administrador" },
                    new Rol { IdRol = 2, NombreRol = "Coordinador" }
                };
                dbContext.Roles.AddRange(roles);
                await dbContext.SaveChangesAsync();
            }
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await ResetDatabaseAsync();
        await base.DisposeAsync();
    }

    /// <summary>
    /// Obtiene un DbContext nuevo dentro de un scope apropiado.
    /// IMPORTANTE: Cada llamada retorna un DbContext FRESCO
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
    /// Obtiene un DbContext nuevo dentro de un scope apropiado para operaciones sin retorno.
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
    /// LIMPIEZA TOTAL: Elimina todos los usuarios (mantiene roles base).
    /// Se ejecuta al final de cada test mediante IAsyncLifetime.
    /// </summary>
    public async Task ResetDatabaseAsync()
    {
        await ExecuteDbContextAsync(async dbContext =>
        {
            // ✅ LIMPIEZA COMPLETA: Eliminar TODOS los usuarios sin excepción
            var usuarios = await dbContext.Usuarios.ToListAsync();
            if (usuarios.Any())
            {
                dbContext.Usuarios.RemoveRange(usuarios);
                await dbContext.SaveChangesAsync();
            }
        });
    }
}

