using BigBang1112.GbxTools.IO.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BigBang1112.GbxTools.IO.Tests;

public class GbxIoExtensionsTests
{
    private readonly IServiceProvider provider = TestServices.Create();

    [Test]
    [Arguments("compress-gbx", typeof(CompressGbxTool))]
    [Arguments("decompress-gbx", typeof(DecompressGbxTool))]
    [Arguments("extract-map-from-replay", typeof(ExtractMapFromReplayTool))]
    [Arguments("extract-thumbnail", typeof(ExtractThumbnailTool))]
    [Arguments("compress-embedded-items", typeof(CompressEmbeddedItemsTool))]
    [Arguments("extract-embedded-items", typeof(ExtractEmbeddedItemsTool))]
    [Arguments("extract-ghosts", typeof(ExtractGhostsTool))]
    [Arguments("extract-mesh", typeof(ExtractMeshTool))]
    [Arguments("extract-inputs", typeof(ExtractInputsTool))]
    [Arguments("extract-inputs-tmi", typeof(ExtractInputsTmiTool))]
    [Arguments("remove-validation-ghost", typeof(RemoveValidationGhostTool))]
    [Arguments("remove-ips-from-replays", typeof(RemoveIpsFromReplaysTool))]
    [Arguments("change-to-old-wood-physics", typeof(ChangeToOldWoodPhysicsTool))]
    [Arguments("validate-without-lightmaps", typeof(ValidateWithoutLightmapsTool))]
    [Arguments("patch-tm2-stadium-bugfinish", typeof(PatchTM2StadiumBugfinishTool))]
    [Arguments("gbx-to-json", typeof(GbxToJsonTool))]
    [Arguments("pak-to-zip", typeof(PakToZipTool))]
    public async Task AddGbxIo_RegistersToolUnderEndpoint(string endpoint, Type toolType)
    {
        // Act
        var tool = provider.GetRequiredService<ToolService>().GetTool(endpoint);

        // Assert
        await Assert.That(tool?.GetType()).IsEqualTo(toolType);
        await Assert.That(tool!.Endpoint).IsEqualTo(endpoint);
    }

    [Test]
    public async Task AddGbxIo_RegistersEveryToolOnceForTheMenu()
    {
        // Act
        var tools = provider.GetServices<IoTool>().ToList();

        // Assert
        await Assert.That(tools.Count).IsEqualTo(17);
        await Assert.That(tools.Select(x => x.Endpoint).Distinct().Count()).IsEqualTo(tools.Count);
    }

    [Test]
    public async Task AddGbxIo_GivesEveryToolAUniqueName()
    {
        // Act
        var names = provider.GetServices<IoTool>().Select(x => x.Name).ToList();

        // Assert
        await Assert.That(names.All(name => !string.IsNullOrWhiteSpace(name))).IsTrue();
        await Assert.That(names.Distinct().Count()).IsEqualTo(names.Count);
    }

    [Test]
    public async Task AddGbxIo_GivesEveryToolInputAndOutputExtensions()
    {
        // Act
        var tools = provider.GetServices<IoTool>().ToList();

        // Assert
        await Assert.That(tools.Where(x => !x.InputExtensions.Any()).Select(x => x.Name)).IsEmpty();
        await Assert.That(tools.Where(x => !x.OutputExtensions.Any()).Select(x => x.Name)).IsEmpty();
    }
}
