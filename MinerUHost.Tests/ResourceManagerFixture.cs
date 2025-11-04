using EasyReasy;
using System.Reflection;

namespace MinerUHost.Tests
{
    public sealed class ResourceManagerFixture : IAsyncLifetime
    {
        public ResourceManager ResourceManager { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(TestFile))!;
            ResourceManager = await ResourceManager.CreateInstanceAsync(assembly);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}

