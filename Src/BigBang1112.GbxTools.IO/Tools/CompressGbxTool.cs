using ByteSizeLib;
using GBX.NET;
using BigBang1112.GbxTools.IO.Data;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class CompressGbxTool(string endpoint, IServiceProvider provider)
    : IoTool<GbxData, GbxData>(endpoint, provider)
{
    public override string Name => "Compress Gbx";

    public override async Task<GbxData> ProcessAsync(GbxData input, ILogger logger, CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream((int)input.Stream.Length);

        await Gbx.CompressAsync(input.Stream, outputStream, cancellationToken);

        var optimizedByteCount = input.Stream.Length - outputStream.Length;

        await ReportAsync($"Compressed by {optimizedByteCount / (double)input.Stream.Length:P} ({ByteSize.FromBytes(optimizedByteCount)})", logger, cancellationToken);

        return new GbxData(input.FileName, outputStream);
    }
}
