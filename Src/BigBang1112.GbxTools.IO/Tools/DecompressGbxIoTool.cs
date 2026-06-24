using ByteSizeLib;
using GBX.NET;
using BigBang1112.GbxTools.IO.Data;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class DecompressGbxIoTool(string endpoint, IServiceProvider provider)
    : IoTool<GbxData, GbxData>(endpoint, provider)
{
    public override string Name => "Decompress Gbx";

    public override async Task<GbxData> ProcessAsync(GbxData input, CancellationToken cancellationToken)
    {
        await using var outputStream = new MemoryStream((int)input.Stream.Length);

        await Gbx.DecompressAsync(input.Stream, outputStream, cancellationToken);

        var sizeIncreased = outputStream.Length - input.Stream.Length;

        await ReportAsync($"Decompressed. File size increased by {ByteSize.FromBytes(sizeIncreased)} ({sizeIncreased / (double)input.Stream.Length:P}).", cancellationToken);

        return new GbxData(input.FileName, outputStream);
    }
}
