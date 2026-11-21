using BigBang1112.GbxTools.IO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BigBang1112.GbxTools.IO.Tests;

internal static class TestServices
{
    /// <summary>
    /// The services the hosts register for Gbx I/O.
    /// </summary>
    public static ServiceProvider Create(Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://gbx-io.test/") });
        services.AddGbxIo();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }

    public static T GetTool<T>(this IServiceProvider provider, string endpoint) where T : IoTool
    {
        return (T)provider.GetRequiredService<ToolService>().GetTool(endpoint)!;
    }
}
