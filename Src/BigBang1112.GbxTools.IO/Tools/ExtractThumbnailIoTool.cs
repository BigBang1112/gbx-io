using BigBang1112.GbxTools.IO.Attributes;
using BigBang1112.GbxTools.IO.Data;
using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Engines.GameData;
using GBX.NET.Imaging.SkiaSharp;
using GBX.NET.Managers;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class ExtractThumbnailIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, BinData?>(endpoint, provider)
{
    public override string Name => "Extract thumbnail/icon";

    public override IEnumerable<string> OutputExtensions => ["jpg", "png"];

    public override async Task<BinData?> ProcessAsync([HeaderOnly] Gbx input, CancellationToken cancellationToken)
    {
        if (input is Gbx<CGameCtnChallenge> gbxMap)
        {
            await using var ms = new MemoryStream();

            if (gbxMap.Node.ExportThumbnail(ms, SkiaSharp.SKEncodedImageFormat.Jpeg, 100))
            {
                return new BinData((input.FilePath ?? "unknown") + ".jpg", ms, "image/jpeg");
            }

            return null;
        }

        if (input.Node is CGameCtnCollector collector)
        {
            await using var ms = new MemoryStream();

            if (collector.ExportIcon(ms))
            {
                return new BinData((input.FilePath ?? "unknown") + ".png", ms, "image/png");
            }
        }

        return null;
    }
}