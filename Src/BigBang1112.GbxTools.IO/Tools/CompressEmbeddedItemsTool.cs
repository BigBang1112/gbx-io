using GBX.NET.Engines.Game;
using GBX.NET;
using ByteSizeLib;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class CompressEmbeddedItemsTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Compress embedded items";

    public override async Task<Gbx<CGameCtnChallenge>> ProcessAsync(Gbx<CGameCtnChallenge> input, ILogger logger, CancellationToken cancellationToken)
    {
        if (input.Node.EmbeddedZipData is null or { Length: 0 })
        {
            if (input.Node.TMUnlimiterData?.EmbeddedBlocks.Count > 0 || input.Node.TMUnlimiterData?.EmbeddedImages.Count > 0)
            {
                throw new InvalidOperationException("TMUnlimiter embedded data cannot be compressed further.");
            }

            throw new InvalidOperationException("No embedded data found.");
        }

        using var inputStream = new MemoryStream(input.Node.EmbeddedZipData);
        using var inputZip = new ZipArchive(inputStream);

        using var outputStream = new MemoryStream();

        var reportedLzoDecompression = false;

        await using (var zipArchive = await SharpCompress.Archives.Zip.ZipArchive.CreateAsyncArchive())
        {
            foreach (var (i, entry) in inputZip.Entries.Index())
            {
                logger.LogInformation("Moving {EntryName} ({EntrySize})...", entry.FullName, ByteSize.FromBytes(entry.Length));

                var ms = new MemoryStream();
                await using var entryStream = entry.Open();

                if (entry.FullName.EndsWith(".gbx", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (!reportedLzoDecompression)
                    {
                        await ReportAsync("Decompressing embedded data from LZO...", logger, cancellationToken);
                        reportedLzoDecompression = true;
                    }
                    await Gbx.DecompressAsync(input: entryStream, output: ms, cancellationToken);
                }
                else
                {
                    entryStream.CopyTo(ms);
                }

                var zipEntry = await zipArchive.AddEntryAsync(entry.FullName, ms, true, cancellationToken: cancellationToken);
            }

            await ReportAsync("Compressing embedded data with DEFLATE...", logger, cancellationToken);

            await zipArchive.SaveToAsync(outputStream, new(SharpCompress.Common.CompressionType.Deflate, SharpCompress.Compressors.Deflate.CompressionLevel.BestCompression), cancellationToken);
        }

        var optimizedByteCount = input.Node.EmbeddedZipData.Length - outputStream.Length;

        await ReportAsync(optimizedByteCount >= 0
            ? $"Embedded data compressed by {optimizedByteCount / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(optimizedByteCount)})"
            : $"Embedded data unfortunately increased by {Math.Abs(optimizedByteCount) / (double)input.Node.EmbeddedZipData.Length:P} ({ByteSize.FromBytes(Math.Abs(optimizedByteCount))})", logger, cancellationToken);

        input.Node.EmbeddedZipData = outputStream.ToArray();

        return input;
    }
}