using Xunit;

namespace ApplicationSchedule.Tests.Infrastructure;

/// <summary>
/// Clase base para pruebas de integración que necesitan cliente HTTP y aislamiento de base de datos.
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;

    protected HttpClient Client { get; private set; } = null!;

    /// <summary>
    /// Inicializa la base de la prueba con la factoría compartida.
    /// </summary>
    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Prepara el cliente HTTP y la base de datos antes de cada prueba.
    /// </summary>
    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        await Factory.InitializeDatabaseAsync();
    }

    /// <summary>
    /// Limpia el estado después de cada prueba.
    /// </summary>
    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }
}