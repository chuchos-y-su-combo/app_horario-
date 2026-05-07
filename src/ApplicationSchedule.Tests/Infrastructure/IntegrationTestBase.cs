using ApplicationSchedule.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// Clase base para todos los tests de integración.
/// 
/// PATRÓN DE USO CORRECTO:
/// 
/// public class UsuariosApiTests : IntegrationTestBase, IClassFixture<CustomWebApplicationFactory>
/// {
///     public UsuariosApiTests(CustomWebApplicationFactory factory) : base(factory) { }
///     
///     [Fact]
///     public async Task TestEjemplo()
///     {
///         // Aquí puedes usar Client para hacer requests HTTP
///         var response = await Client.GetAsync("/api/usuarios");
///         Assert.Equal(200, (int)response.StatusCode);
///     }
/// }
/// 
/// GARANTÍAS:
/// ✅ Cada test recibe su propia Factory con BD en memoria independiente
/// ✅ InitializeAsync() se ejecuta automáticamente ANTES de cada test
/// ✅ DisposeAsync() se ejecuta automáticamente DESPUÉS de cada test
/// ✅ BD siempre comienza limpia con roles base
/// ✅ Tests no interfieren entre sí
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
    /// Llamado automáticamente por xUnit DESPUÉS de cada método [Fact].
    /// Aquí se limpia la BD para que el siguiente test comience en estado limpio.
    /// </summary>
    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }
}
