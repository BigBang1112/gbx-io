using ByteSizeLib;
using GBX.NET;
using BigBang1112.GbxTools.IO.Data;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class DecompressGbxTool(string endpoint, IServiceProvider provider)
    : IoTool<GbxData, GbxData>(endpoint, provider)
{
    public override string Name => "Decompress Gbx";

    public override async Task<GbxData> ProcessAsync(GbxData input, ILogger logger, CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream((int)input.Stream.Length);

        await Gbx.DecompressAsync(input.Stream, outputStream, cancellationToken);

        var sizeIncreased = outputStream.Length - input.Stream.Length;

        await ReportAsync($"Decompressed - file size increased by {ByteSize.FromBytes(sizeIncreased)} ({sizeIncreased / (double)input.Stream.Length:P})", logger, cancellationToken);

        return new GbxData(input.FileName, outputStream);
    }
}
