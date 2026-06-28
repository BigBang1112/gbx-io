using BigBang1112.GbxTools.IO.Services;
using BigBang1112.GbxTools.IO.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace BigBang1112.GbxTools.IO;

public static class GbxIoExtensions
{
    public static IServiceCollection AddGbxIo(this IServiceCollection services)
    {
        services.AddScoped<GbxService>();
        services.AddScoped<ToolService>();

        services.AddTool<CompressGbxIoTool>("compress-gbx");
        services.AddTool<DecompressGbxIoTool>("decompress-gbx");
        services.AddTool<ExtractMapFromReplayIoTool>("extract-map-from-replay");
        services.AddTool<ExtractThumbnailIoTool>("extract-thumbnail");
        services.AddTool<CompressEmbeddedItemsIoTool>("compress-embedded-items");
        services.AddTool<ExtractEmbeddedItemsIoTool>("extract-embedded-items");
        services.AddTool<ExtractGhostsIoTool>("extract-ghosts");
        services.AddTool<ExtractMeshIoTool>("extract-mesh");
        services.AddTool<ExtractInputsIoTool>("extract-inputs");
        services.AddTool<ExtractInputsTmiIoTool>("extract-inputs-tmi");
        services.AddTool<ChangeToOldWoodPhysicsIoTool>("change-to-old-wood-physics");
        services.AddTool<ValidateWithoutLightmapsIoTool>("validate-without-lightmaps");
        services.AddTool<PatchTM2StadiumBugfinishIoTool>("patch-tm2-stadium-bugfinish");
        services.AddTool<GbxToJsonIoTool>("gbx-to-json");
        services.AddTool<PakToZipTool>("pak-to-zip");

        return services;
    }

    private static IServiceCollection AddTool<T>(this IServiceCollection services, string key)
        where T : IoTool
    {
        ArgumentNullException.ThrowIfNull(key);

        services.AddKeyedScoped<IoTool, T>(key, (provider, key) => (T)Activator.CreateInstance(typeof(T), key!.ToString(), provider)!);
        services.AddScoped(provider => provider.GetRequiredKeyedService<IoTool>(key));

        return services;
    }
}