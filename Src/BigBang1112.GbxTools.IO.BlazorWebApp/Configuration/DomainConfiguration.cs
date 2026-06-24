using BigBang1112.GbxTools.IO.BlazorWebApp.Client.Services;
using BigBang1112.GbxTools.IO.Services;

namespace BigBang1112.GbxTools.IO.BlazorWebApp.Configuration;

internal static class DomainConfiguration
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddGbxIo();
        services.AddTransient<IDownloadService, DownloadService>();
    }
}