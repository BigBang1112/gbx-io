using System.Runtime.InteropServices;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class RemoveIpsFromReplaysToolTests
{
    private static readonly byte[] Ip = [192, 168, 1, 42];

    private readonly RemoveIpsFromReplaysTool tool = TestServices.Create().GetTool<RemoveIpsFromReplaysTool>("remove-ips-from-replays");

    /// <summary>
    /// The skippable chunk 0x03092023 that holds the IP, 20 bytes after its header.
    /// </summary>
    private static byte[] CreateIpRecord(byte[] ip)
    {
        ReadOnlySpan<int> header = [0x03092023, 0x534B4950, 47, 3, 0];
        return [.. MemoryMarshal.AsBytes(header), .. Enumerable.Repeat((byte)0xAA, 20), .. ip];
    }

    [Test]
    public async Task ClearIps_WithRecord_ClearsOnlyTheIp()
    {
        // Arrange
        byte[] data = [1, 2, 3, .. CreateIpRecord(Ip), 4, 5, 6];
        var expected = data.ToArray();
        expected.AsSpan(3 + 40, 4).Clear();

        // Act
        var count = RemoveIpsFromReplaysTool.ClearIps(data);

        // Assert
        await Assert.That(count).IsEqualTo(1);
        await Assert.That(data).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ClearIps_WithMultipleRecords_ClearsAll()
    {
        // Arrange
        byte[] data = [.. CreateIpRecord(Ip), 7, .. CreateIpRecord([10, 0, 0, 1])];

        // Act
        var count = RemoveIpsFromReplaysTool.ClearIps(data);

        // Assert
        await Assert.That(count).IsEqualTo(2);
        await Assert.That(data.AsSpan(40, 4).ToArray()).IsEquivalentTo(new byte[4]);
        await Assert.That(data.AsSpan(45 + 40, 4).ToArray()).IsEquivalentTo(new byte[4]);
    }

    [Test]
    public async Task ClearIps_WithTruncatedRecord_LeavesDataAlone()
    {
        // Arrange
        var data = CreateIpRecord(Ip)[..^1];
        var expected = data.ToArray();

        // Act
        var count = RemoveIpsFromReplaysTool.ClearIps(data);

        // Assert
        await Assert.That(count).IsEqualTo(0);
        await Assert.That(data).IsEquivalentTo(expected);
    }

    [Test]
    public async Task ClearIps_WithoutRecord_ReturnsZero()
    {
        // Arrange
        var data = Enumerable.Range(0, 256).Select(x => (byte)x).ToArray();

        // Act
        var count = RemoveIpsFromReplaysTool.ClearIps(data);

        // Assert
        await Assert.That(count).IsEqualTo(0);
    }

    [Test]
    public async Task ProcessAsync_WithIp_ReturnsCompressedGbxWithoutIp()
    {
        // Arrange
        var body = await TestGbx.DecompressAsync(TestGbx.SaveMap());
        body.Seek(0, SeekOrigin.End);
        body.Write(CreateIpRecord(Ip));
        body.Position = 0;
        var input = new GbxData("Test.Replay.Gbx", await TestGbx.CompressAsync(body));

        var progress = new TestProgress();
        tool.Progress = progress;

        // Act
        var output = await tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None);

        // Assert
        output.Stream.Position = 0;
        var result = (await TestGbx.DecompressAsync(output.Stream)).ToArray();

        await Assert.That(output.FileName).IsEqualTo("Test.Replay.Gbx");
        await Assert.That(result.AsSpan(result.Length - 4).ToArray()).IsEquivalentTo(new byte[4]);
        await Assert.That(result.AsSpan(0, result.Length - 4).ToArray()).IsEquivalentTo(body.ToArray()[..^4]);
        await Assert.That(progress.Messages).IsEquivalentTo(["Removed 1 IP"]);
    }

    [Test]
    public async Task ProcessAsync_WithoutIp_Throws()
    {
        // Arrange
        var input = new GbxData("Test.Replay.Gbx", TestGbx.SaveMap());

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("No IPs found in this replay.");
    }
}
