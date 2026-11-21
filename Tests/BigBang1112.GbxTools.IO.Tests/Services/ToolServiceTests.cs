using BigBang1112.GbxTools.IO.Exceptions;
using BigBang1112.GbxTools.IO.Services;
using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.Extensions.DependencyInjection;

namespace BigBang1112.GbxTools.IO.Tests.Services;

public class ToolServiceTests
{
    private readonly ToolService toolService = TestServices.Create().GetRequiredService<ToolService>();

    [Test]
    public async Task GetTool_WithKnownEndpoint_ReturnsTool()
    {
        // Act
        var tool = toolService.GetTool("compress-gbx");

        // Assert
        await Assert.That(tool).IsTypeOf<CompressGbxTool>();
    }

    [Test]
    public async Task GetTool_WithUnknownEndpoint_ReturnsNull()
    {
        // Act
        var tool = toolService.GetTool("unknown");

        // Assert
        await Assert.That(tool).IsNull();
    }

    [Test]
    public async Task ProcessFileAsync_WithUnknownTool_ReturnsNull()
    {
        // Arrange
        var data = new BinData("file.bin", new MemoryStream([1, 2, 3]), "application/octet-stream");

        // Act
        var output = await toolService.ProcessFileAsync("unknown", data, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsNull();
    }

    [Test]
    public async Task ProcessFileAsync_WithGbxDataTool_PassesFileThrough()
    {
        // Arrange
        var decompressed = await TestGbx.DecompressAsync(TestGbx.SaveMap());
        var data = new BinData("Test.Map.Gbx", decompressed, "application/octet-stream");

        // Act
        var output = await toolService.ProcessFileAsync("compress-gbx", data, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsTypeOf<GbxData>();
        var gbxData = (GbxData)output!;
        await Assert.That(gbxData.FileName).IsEqualTo("Test.Map.Gbx");
        await Assert.That(gbxData.Stream.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task ProcessFileAsync_WithTypedGbxTool_ParsesGbxAndKeepsFileName()
    {
        // Arrange
        var data = new BinData("Uploaded.Map.Gbx", TestGbx.SaveMap(), "application/octet-stream");

        // Act
        var output = await toolService.ProcessFileAsync("remove-validation-ghost", data, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsTypeOf<Gbx<CGameCtnChallenge>>();
        await Assert.That(((Gbx)output!).FilePath).IsEqualTo("Uploaded.Map.Gbx");
    }

    [Test]
    public async Task ProcessFileAsync_WithTypedGbxToolAndOtherGbx_ThrowsUnmatchingInput()
    {
        // Arrange
        var ghost = TestGbx.Save(new Gbx<CGameCtnGhost>(new CGameCtnGhost()));
        var data = new BinData("Test.Ghost.Gbx", ghost, "application/octet-stream");

        // Act & Assert
        await Assert.That(() => toolService.ProcessFileAsync("remove-validation-ghost", data, NullLogger.Instance, CancellationToken.None))
            .Throws<UnmatchingInputException>();
    }

    [Test]
    public async Task ProcessFileAsync_WithGbxToolAndNotAGbx_ReturnsNull()
    {
        // Arrange
        var data = new BinData("file.txt", new MemoryStream("not a Gbx file"u8.ToArray()), "text/plain");

        // Act
        var output = await toolService.ProcessFileAsync("remove-validation-ghost", data, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsNull();
    }

    [Test]
    public async Task ProcessFileAsync_WithHeaderOnlyTool_ParsesHeader()
    {
        // Arrange
        var data = new BinData("Test.Map.Gbx", TestGbx.SaveMap(), "application/octet-stream");

        // Act
        var output = await toolService.ProcessFileAsync("extract-thumbnail", data, NullLogger.Instance, CancellationToken.None);

        // Assert, as the map has no thumbnail
        await Assert.That(output).IsNull();
    }

    [Test]
    public async Task GetIoToolBaseType_WithIndirectSubclass_ReturnsGenericBase()
    {
        // Act
        var baseType = ToolService.GetIoToolBaseType(typeof(ExtractInputsTmiTool));

        // Assert
        await Assert.That(baseType).IsEqualTo(typeof(IoTool<Gbx, IEnumerable<TextData>>));
    }

    [Test]
    public async Task GetIoToolBaseType_WithOtherType_ReturnsNull()
    {
        // Act
        var baseType = ToolService.GetIoToolBaseType(typeof(string));

        // Assert
        await Assert.That(baseType).IsNull();
    }
}
