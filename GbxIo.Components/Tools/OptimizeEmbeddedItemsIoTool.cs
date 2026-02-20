using GBX.NET.Engines.Game;
using GBX.NET;
using ByteSizeLib;
using SharpCompress.Archives.Zip;

namespace GbxIo.Components.Tools;

public sealed class OptimizeEmbeddedItemsIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Optimize embedded items";

    public override async Task<Gbx<CGameCtnChallenge>> ProcessAsync(Gbx<CGameCtnChallenge> input, CancellationToken cancellationToken)
    {
        if (input.Node.EmbeddedZipData is null || input.Node.EmbeddedZipData.Length == 0)
        {
            throw new InvalidOperationException("No embedded items found.");
        }

        using var inputStream = new MemoryStream(input.Node.EmbeddedZipData);
        await using var inputZip = await ZipArchive.OpenAsyncArchive(inputStream, cancellationToken: cancellationToken);

        using var outputStream = new MemoryStream();

        await using (var zipArchive = await ZipArchive.CreateAsyncArchive())
        {
            await foreach (var entry in inputZip.EntriesAsync)
            {
                var ms = new MemoryStream();
                await using var entryStream = await entry.OpenEntryStreamAsync(cancellationToken);

                if (entry.Key?.EndsWith(".gbx", StringComparison.OrdinalIgnoreCase) == true)
                {
                    using var inputMs = new MemoryStream();
                    await entryStream.CopyToAsync(inputMs, cancellationToken); // due to a few bugs in GBX.NET
                    inputMs.Position = 0;
                    await Gbx.DecompressAsync(input: inputMs, output: ms, cancellationToken: cancellationToken);
                }
                else
                {
                    await entryStream.CopyToAsync(ms, cancellationToken);
                }

                var zipEntry = await zipArchive.AddEntryAsync(entry.Key!, ms, true, cancellationToken: cancellationToken);
            }

            await zipArchive.SaveToAsync(outputStream, new(SharpCompress.Common.CompressionType.Deflate)
            {
                CompressionLevel = 9
            }, cancellationToken: cancellationToken);
        }

        var optimizedByteCount = input.Node.EmbeddedZipData.Length - outputStream.Length;

        await ReportAsync(optimizedByteCount >= 0
            ? $"Embedded data optimized by {optimizedByteCount / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(optimizedByteCount)})."
            : $"Embedded data unfortunately increased by {Math.Abs(optimizedByteCount) / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(Math.Abs(optimizedByteCount))}).", cancellationToken);

        input.Node.EmbeddedZipData = outputStream.ToArray();

        return input;
    }
}