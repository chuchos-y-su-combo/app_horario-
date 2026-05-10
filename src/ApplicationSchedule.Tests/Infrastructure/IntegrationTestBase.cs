using Xunit;

namespace ApplicationSchedule.Tests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;

    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        await Factory.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }
}