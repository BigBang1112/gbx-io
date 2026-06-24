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
        await using var inputStream = new MemoryStream(input.Data);
        await using var outputStream = new MemoryStream(input.Data.Length);

        await Gbx.CompressAsync(inputStream, outputStream, cancellationToken);

        var optimizedByteCount = inputStream.Length - outputStream.Length;

        await ReportAsync($"Compressed by {optimizedByteCount / (double)inputStream.Length:P} ({ByteSize.FromBytes(optimizedByteCount)}).", cancellationToken);

        return new GbxData(input.FileName, outputStream.ToArray());
    }
}
