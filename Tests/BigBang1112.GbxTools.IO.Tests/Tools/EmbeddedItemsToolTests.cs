using System.IO.Compression;
using System.Text;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class EmbeddedItemsToolTests
{
    private readonly IServiceProvider provider = TestServices.Create();

    private static byte[] CreateZip(params (string Name, string Content)[] entries)
    {
        using var ms = new MemoryStream();

        using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, content) in entries)
            {
                using var writer = new StreamWriter(zip.CreateEntry(name, CompressionLevel.NoCompression).Open(), Encoding.UTF8);
                writer.Write(content);
            }
        }

        return ms.ToArray();
    }

    private static Dictionary<string, string> ReadZip(Stream stream)
    {
        using var zip = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
        return zip.Entries.ToDictionary(x => x.FullName, x =>
        {
            using var reader = new StreamReader(x.Open(), Encoding.UTF8);
            return reader.ReadToEnd();
        });
    }

    [Test]
    public async Task ExtractEmbeddedItems_ReturnsEmbeddedZipNamedAfterMap()
    {
        // Arrange
        var tool = provider.GetTool<ExtractEmbeddedItemsTool>("extract-embedded-items");
        var map = TestGbx.CreateMap("Folder/Test.Map.Gbx");
        map.Node.EmbeddedZipData = CreateZip(("Items/Tree.txt", "tree"));

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.FileName).IsEqualTo("Test.zip");
        await Assert.That(ReadZip(output.Stream)).ContainsKey("Items/Tree.txt");
    }

    [Test]
    public async Task ExtractEmbeddedItems_WithoutEmbeddedData_Throws()
    {
        // Arrange
        var tool = provider.GetTool<ExtractEmbeddedItemsTool>("extract-embedded-items");
        var map = TestGbx.CreateMap();

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("No embedded items found.");
    }

    [Test]
    public async Task CompressEmbeddedItems_KeepsEntriesAndShrinksData()
    {
        // Arrange
        var tool = provider.GetTool<CompressEmbeddedItemsTool>("compress-embedded-items");
        var progress = new TestProgress();
        tool.Progress = progress;

        var content = string.Concat(Enumerable.Repeat("compressible ", 1000));
        var map = TestGbx.CreateMap();
        map.Node.EmbeddedZipData = CreateZip(("Items/A.txt", content), ("Items/B.txt", "b"));
        var originalLength = map.Node.EmbeddedZipData.Length;

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        var entries = ReadZip(new MemoryStream(output.Node.EmbeddedZipData!));
        await Assert.That(entries["Items/A.txt"]).IsEqualTo(content);
        await Assert.That(entries["Items/B.txt"]).IsEqualTo("b");
        await Assert.That(output.Node.EmbeddedZipData!.Length).IsLessThan(originalLength);
        await Assert.That(progress.Messages[^1]).StartsWith("Embedded data compressed by ");
    }

    [Test]
    public async Task CompressEmbeddedItems_WithoutEmbeddedData_Throws()
    {
        // Arrange
        var tool = provider.GetTool<CompressEmbeddedItemsTool>("compress-embedded-items");
        var map = TestGbx.CreateMap();

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("No embedded data found.");
    }
}
