using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// PATRÓN CORRECTO para tests de integración con WebApplicationFactory.
/// 
/// ⚠️ IMPORTANTE - FORMA DE USO:
/// 
/// public class UsuariosApiTests : IClassFixture<CustomWebApplicationFactory>
/// {
///     private readonly CustomWebApplicationFactory _factory;
///     private readonly HttpClient _client;
///     
///     // ✅ xUnit inyecta la Factory en el constructor
///     public UsuariosApiTests(CustomWebApplicationFactory factory)
///     {
///         _factory = factory;
///         _client = factory.CreateClient();
///     }
///     
///     [Fact]
///     public async Task ObtenerTodos_RetornListaVacia()
///     {
///         // Aquí _factory y _client están listos para usar
///         var response = await _client.GetAsync("/api/usuarios");
///         Assert.Equal(200, (int)response.StatusCode);
///     }
/// }
/// 
/// GARANTÍAS DE AISLAMIENTO:
/// ✅ Cada test recibe su propia Factory con BD en memoria ÚNICA (GUID + timestamp)
/// ✅ Cada test hereda de este base y obtiene InitializeAsync() y DisposeAsync() automáticos
/// ✅ xUnit maneja la inyección de dependencias correctamente
/// ✅ No hay conflictos de timing entre inicialización del host y creación de HttpClient
/// ✅ Tests pueden ejecutarse en paralelo sin interferencias
/// ✅ Determinísticos: BD siempre comienza con roles base solamente
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    /// <summary>
    /// Factory que crea la app con BD en memoria. Única por test.
    /// Injected por xUnit cuando usas [ClassFixture<CustomWebApplicationFactory>]
    /// </summary>
    protected readonly CustomWebApplicationFactory Factory;

    /// <summary>
    /// HttpClient preconfigurado apuntando directamente a la app en memoria.
    /// Inicializado en InitializeAsync (no en constructor).
    /// </summary>
    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Constructor que recibe la Factory injected por xUnit.
    /// ⚠️ NO crear HttpClient aquí - esperar a InitializeAsync
    /// </summary>
    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Llamado automáticamente por xUnit ANTES de cada método [Fact].
    /// Aquí se crea HttpClient y se inicializa la BD con roles base.
    /// 
    /// TIMING CORRECTO:
    /// 1. WebApplicationFactory está completamente inicializado
    /// 2. Services están registrados correctamente (DbContext con InMemory)
    /// 3. Safe to create HttpClient y acceder a BD
    /// </summary>
    public async Task InitializeAsync()
    {
        // ✅ Crear HttpClient aquí (AFTER factory is ready)
        Client = Factory.CreateClient();
        
        // ✅ Inicializar BD: crea esquema y siembra roles base
        await Factory.InitializeDatabaseAsync();
    }

    /// <summary>
    /// Llamado automáticamente por xUnit DESPUÉS de cada test [Fact].
    /// Limpia usuarios pero mantiene roles base para el siguiente test.
    /// 
    /// LIMPIEZA:
    /// - Elimina todos los Usuarios creados durante el test
    /// - Mantiene Roles (datos base obligatorios)
    /// - Cierra recursos
    /// </summary>
    public async Task DisposeAsync()
    {
        // ✅ Limpiar usuarios del test (pero mantener roles base)
        await Factory.ResetDatabaseAsync();

        // ✅ Liberar HttpClient
        Client?.Dispose();
    }

    /// <summary>
    /// Helper para acceder a la BD directamente en un scope de DI correcto.
    /// IMPORTANTE: Retorna un DbContext NUEVO cada vez (no reutiliza).
    /// 
    /// Uso: Para verificaciones adicionales o seeds complejas en un test.
    /// 
    /// Ejemplo:
    /// 
    /// await ExecuteDbContextAsync(async dbContext =>
    /// {
    ///     var usuariosCount = await dbContext.Usuarios.CountAsync();
    ///     Assert.Equal(0, usuariosCount);
    /// });
    /// </summary>
    protected async Task<T> ExecuteDbContextAsync<T>(Func<AppDbContext, Task<T>> func)
    {
        return await Factory.ExecuteDbContextAsync(func);
    }
}
