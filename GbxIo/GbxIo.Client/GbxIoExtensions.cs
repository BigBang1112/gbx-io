﻿using GbxIo.Client.Services;
using GbxIo.Client.Tools;
using GbxIo.Components.Services;

namespace GbxIo.Client;

public static class GbxIoExtensions
{
    public static IServiceCollection AddGbxIo(this IServiceCollection services)
    {
        services.AddScoped<GbxService>();
        services.AddScoped<ToolService>();

        services.AddTool<OptimizeGbxIoTool>("optimize-gbx");
        services.AddTool<DecompressGbxIoTool>("decompress-gbx");
        services.AddTool<ExtractMapFromReplayIoTool>("extract-map-from-replay");
        services.AddTool<ExtractThumbnailIoTool>("extract-thumbnail");
        services.AddTool<OptimizeEmbeddedItemsIoTool>("optimize-embedded-items");
        services.AddTool<ExtractEmbeddedItemsIoTool>("extract-embedded-items");
        services.AddTool<ExtractGhostsIoTool>("extract-ghosts");
        services.AddTool<ExtractMeshIoTool>("extract-mesh");
        services.AddTool<ExtractInputsIoTool>("extract-inputs");
        services.AddTool<ExtractInputsTmiIoTool>("extract-inputs-tmi");
        services.AddTool<ChangeToOldWoodPhysicsIoTool>("change-to-old-wood-physics");
        services.AddTool<ValidateWithoutLightmapsIoTool>("validate-without-lightmaps");
        services.AddTool<GbxToJsonIoTool>("gbx-to-json");
        services.AddTool<PakToZipTool>("pak-to-zip");
        services.AddTool<PakToZipVsk5Tool>("pak-to-zip-vsk5");

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
