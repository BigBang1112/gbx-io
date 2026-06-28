using ByteSizeLib;
using GBX.NET;
using BigBang1112.GbxTools.IO.Data;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class CompressGbxIoTool(string endpoint, IServiceProvider provider)
    : IoTool<GbxData, GbxData>(endpoint, provider)
{
    public override string Name => "Compress Gbx";

    public override async Task<GbxData> ProcessAsync(GbxData input, CancellationToken cancellationToken)
    {
        var outputStream = new MemoryStream((int)input.Stream.Length);

        await Gbx.CompressAsync(input.Stream, outputStream, cancellationToken);

        var optimizedByteCount = input.Stream.Length - outputStream.Length;

        await ReportAsync($"Compressed by {optimizedByteCount / (double)input.Stream.Length:P} ({ByteSize.FromBytes(optimizedByteCount)}).", cancellationToken);

        return new GbxData(input.FileName, outputStream);
    }
}
