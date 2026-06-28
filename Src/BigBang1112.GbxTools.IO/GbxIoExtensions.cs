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

        services.AddTool<CompressGbxTool>("compress-gbx");
        services.AddTool<DecompressGbxTool>("decompress-gbx");
        services.AddTool<ExtractMapFromReplayTool>("extract-map-from-replay");
        services.AddTool<ExtractThumbnailTool>("extract-thumbnail");
        services.AddTool<CompressEmbeddedItemsTool>("compress-embedded-items");
        services.AddTool<ExtractEmbeddedItemsTool>("extract-embedded-items");
        services.AddTool<ExtractGhostsTool>("extract-ghosts");
        services.AddTool<ExtractMeshTool>("extract-mesh");
        services.AddTool<ExtractInputsTool>("extract-inputs");
        services.AddTool<ExtractInputsTmiTool>("extract-inputs-tmi");
        services.AddTool<ChangeToOldWoodPhysicsTool>("change-to-old-wood-physics");
        services.AddTool<ValidateWithoutLightmapsTool>("validate-without-lightmaps");
        services.AddTool<PatchTM2StadiumBugfinishTool>("patch-tm2-stadium-bugfinish");
        services.AddTool<GbxToJsonTool>("gbx-to-json");
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