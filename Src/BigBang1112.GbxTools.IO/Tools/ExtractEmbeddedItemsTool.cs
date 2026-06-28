using GBX.NET;
using GBX.NET.Engines.Game;
using BigBang1112.GbxTools.IO.Data;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class ExtractEmbeddedItemsTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, ZipData>(endpoint, provider)
{
    public override string Name => "Extract embedded items";

    public override Task<ZipData> ProcessAsync(Gbx<CGameCtnChallenge> input, CancellationToken cancellationToken)
    {
        if (input.Node.EmbeddedZipData is null or { Length: 0 })
        {
            throw new InvalidOperationException("No embedded items found.");
        }

        var fileName = GbxPath.GetFileNameWithoutExtension(input.FilePath) + ".zip";

        var zipData = new ZipData(fileName, input.Node.EmbeddedZipData);

        return Task.FromResult(zipData);
    }
}
