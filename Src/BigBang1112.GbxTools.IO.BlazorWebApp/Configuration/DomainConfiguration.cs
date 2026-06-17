using BigBang1112.GbxTools.IO;

namespace BigBang1112.GbxTools.IO.BlazorWebApp.Configuration;

internal static class DomainConfiguration
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddGbxIo();
    }
}