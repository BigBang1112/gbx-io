namespace GbxIo.BlazorWebApp.Configuration;

internal static class DomainConfiguration
{
    public static void AddDomainServices(this IServiceCollection services)
    {
        services.AddGbxIo();
    }
}