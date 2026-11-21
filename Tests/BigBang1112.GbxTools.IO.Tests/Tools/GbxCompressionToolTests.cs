using GBX.NET;
using GBX.NET.Engines.Game;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class GbxCompressionToolTests
{
    private readonly IServiceProvider provider = TestServices.Create();

    [Test]
    public async Task DecompressGbx_ReturnsUncompressedGbx()
    {
        // Arrange
        var tool = provider.GetTool<DecompressGbxTool>("decompress-gbx");
        var input = new GbxData("Test.Map.Gbx", TestGbx.SaveMap());

        // Act
        var output = await tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None);

        // Assert
        var bytes = ((MemoryStream)output.Stream).ToArray();
        await Assert.That(output.FileName).IsEqualTo("Test.Map.Gbx");
        await Assert.That((char)bytes[TestGbx.BodyCompressionIndex]).IsEqualTo('U');
    }

    [Test]
    public async Task CompressGbx_ReturnsCompressedGbx()
    {
        // Arrange
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");
        var input = new GbxData("Test.Map.Gbx", await TestGbx.DecompressAsync(TestGbx.SaveMap()));

        // Act
        var output = await tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None);

        // Assert
        var bytes = ((MemoryStream)output.Stream).ToArray();
        await Assert.That(output.FileName).IsEqualTo("Test.Map.Gbx");
        await Assert.That((char)bytes[TestGbx.BodyCompressionIndex]).IsEqualTo('C');
    }

    [Test]
    public async Task DecompressThenCompress_ReturnsReadableGbx()
    {
        // Arrange
        var decompress = provider.GetTool<DecompressGbxTool>("decompress-gbx");
        var compress = provider.GetTool<CompressGbxTool>("compress-gbx");
        var input = new GbxData("Test.Map.Gbx", TestGbx.Save(TestGbx.CreateMap()));

        // Act
        var decompressed = await decompress.ProcessAsync(input, NullLogger.Instance, CancellationToken.None);
        decompressed.Stream.Position = 0;
        var output = await compress.ProcessAsync(decompressed, NullLogger.Instance, CancellationToken.None);

        // Assert
        output.Stream.Position = 0;
        var gbx = await Gbx.ParseAsync(output.Stream);
        await Assert.That(gbx).IsTypeOf<Gbx<CGameCtnChallenge>>();
    }

    [Test]
    public async Task CompressGbx_ReportsSizeChange()
    {
        // Arrange
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");
        var progress = new TestProgress();
        tool.Progress = progress;
        var input = new GbxData("Test.Map.Gbx", await TestGbx.DecompressAsync(TestGbx.SaveMap()));

        // Act
        await tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(progress.Messages.Count).IsEqualTo(1);
        await Assert.That(progress.Messages[0]).StartsWith("Compressed by ");
    }
}
