using BigBang1112.GbxTools.IO.Data;
using GBX.NET;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class RemoveIpsFromReplaysTool(string endpoint, IServiceProvider provider)
    : IoTool<GbxData, GbxData>(endpoint, provider)
{
    private const int IpOffsetAfterPattern = 20;
    private const int IpLength = 4;

    public override string Name => "Remove IPs from replays";

    public override IEnumerable<string> InputExtensions => ["Replay.Gbx", "Ghost.Gbx"];
    public override IEnumerable<string> OutputExtensions => ["Replay.Gbx", "Ghost.Gbx"];

    public override async Task<GbxData> ProcessAsync(GbxData input, ILogger logger, CancellationToken cancellationToken)
    {
        using var ms = new MemoryStream();

        await Gbx.DecompressAsync(input.Stream, ms, cancellationToken);

        var removedCount = ClearIps(ms.GetBuffer().AsSpan(0, checked((int)ms.Length)));

        if (removedCount == 0)
        {
            throw new InvalidOperationException("No IPs found in this replay.");
        }

        ms.Position = 0;

        var outputStream = new MemoryStream((int)ms.Length);

        await Gbx.CompressAsync(ms, outputStream, cancellationToken);

        await ReportAsync($"Removed {removedCount} IP{(removedCount == 1 ? "" : "s")}", logger, cancellationToken);

        return new GbxData(input.FileName, outputStream);
    }

    private static int ClearIps(Span<byte> data)
    {
        ReadOnlySpan<int> patternInts = [0x03092023, 0x534B4950, 47, 3, 0];
        var pattern = MemoryMarshal.AsBytes(patternInts);

        var count = 0;
        var offset = 0;

        while (offset < data.Length)
        {
            var index = data[offset..].IndexOf(pattern);

            if (index < 0)
            {
                break;
            }

            index += offset;

            var ipIndex = index + pattern.Length + IpOffsetAfterPattern;

            if (ipIndex + IpLength <= data.Length)
            {
                data.Slice(ipIndex, IpLength).Clear();
                count++;
            }

            offset = index + pattern.Length;
        }

        return count;
    }
}
