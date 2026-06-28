using GBX.NET.Engines.Game;
using GBX.NET;
using ByteSizeLib;
using System.IO.Compression;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class CompressEmbeddedItemsIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Compress embedded items";

    public override async Task<Gbx<CGameCtnChallenge>> ProcessAsync(Gbx<CGameCtnChallenge> input, CancellationToken cancellationToken)
    {
        if (input.Node.EmbeddedZipData is null || input.Node.EmbeddedZipData.Length == 0)
        {
            throw new InvalidOperationException("No embedded items found.");
        }

        using var inputStream = new MemoryStream(input.Node.EmbeddedZipData);
        using var inputZip = new ZipArchive(inputStream);

        using var outputStream = new MemoryStream();

        using (var zipArchive = SharpCompress.Archives.Zip.ZipArchive.CreateArchive())
        {
            foreach (var entry in inputZip.Entries)
            {
                var ms = new MemoryStream();
                await using var entryStream = entry.Open();

                if (entry.FullName.EndsWith(".gbx", StringComparison.OrdinalIgnoreCase) == true)
                {
                    await Gbx.DecompressAsync(input: entryStream, output: ms, cancellationToken);
                }
                else
                {
                    entryStream.CopyTo(ms);
                }

                var zipEntry = zipArchive.AddEntry(entry.FullName, ms, true);
            }

            zipArchive.SaveTo(outputStream, new(SharpCompress.Common.CompressionType.Deflate, SharpCompress.Compressors.Deflate.CompressionLevel.BestCompression));
        }

        var optimizedByteCount = input.Node.EmbeddedZipData.Length - outputStream.Length;

        await ReportAsync(optimizedByteCount >= 0
            ? $"Embedded data compressed by {optimizedByteCount / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(optimizedByteCount)})."
            : $"Embedded data unfortunately increased by {Math.Abs(optimizedByteCount) / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(Math.Abs(optimizedByteCount))}).", cancellationToken);

        input.Node.EmbeddedZipData = outputStream.ToArray();

        return input;
    }
}