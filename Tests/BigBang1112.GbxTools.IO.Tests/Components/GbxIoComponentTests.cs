using GBX.NET;
using System.IO.Compression;
using Reuniverse.Razor;

namespace BigBang1112.GbxTools.IO.Tests.Components;

public class GbxIoComponentTests : ComponentTestBase
{
    private static UploadEventArgs CreateUpload(string fileName, byte[] data, string type = "application/octet-stream")
    {
        return new UploadEventArgs(fileName, data, DateTimeOffset.UnixEpoch, type);
    }

    private static Task UploadAsync(IRenderedComponent<GbxIoComponent> cut, UploadEventArgs upload)
    {
        var callback = cut.FindComponent<ReuUploadScreen>().Instance.OnUpload;
        return cut.InvokeAsync(() => callback.InvokeAsync(upload));
    }

    [Test]
    public async Task Render_WithoutTool_AsksToSelectTool()
    {
        // Act
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, null));

        // Assert
        await Assert.That(cut.Find("h1").TextContent).IsEqualTo("Please select a tool");
    }

    [Test]
    public async Task Render_WithTool_ShowsToolAndExtensions()
    {
        // Act
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "extract-embedded-items"));

        // Assert
        var extensions = cut.FindAll(".gbx-item .extensions");
        await Assert.That(cut.Find("h1").TextContent).IsEqualTo("Extract embedded items");
        await Assert.That(extensions[1].TextContent.Trim()).IsEqualTo("*.zip");
    }

    [Test]
    public async Task Render_WithTool_AcceptsOnlyInputExtensions()
    {
        // Act
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "pak-to-zip"));

        // Assert
        await Assert.That(cut.FindComponent<ReuUploadScreen>().Instance.Accept).IsEqualTo(".pak,.Pack.Gbx");
    }

    [Test]
    public async Task Render_ScrollsToolMenuToActiveTool()
    {
        // Act
        Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "gbx-to-json"));

        // Assert
        await Assert.That(JSInterop.Invocations.Select(x => x.Identifier)).Contains("window.gbxIo.scrollToolMenuToActive");
    }

    [Test]
    public async Task SelectTool_ShowsToolAndUpdatesUrl()
    {
        // Arrange
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, null));

        // Act
        cut.FindAll(".tool-menu button").Single(x => x.TextContent.Trim() == "Decompress Gbx").Click();

        // Assert
        var pushState = JSInterop.Invocations["history.pushState"].Single();
        await Assert.That(cut.Find("h1").TextContent).IsEqualTo("Decompress Gbx");
        await Assert.That(pushState.Arguments[2]).IsEqualTo("/decompress-gbx");
    }

    [Test]
    public async Task Upload_ProcessesFileAndDownloadsOutput()
    {
        // Arrange
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "decompress-gbx"));

        // Act
        await UploadAsync(cut, CreateUpload("Test.Map.Gbx", TestGbx.SaveMap().ToArray()));

        // Assert
        await Assert.That(Downloads.Files.Count).IsEqualTo(1);
        await Assert.That(Downloads.Files[0].FileName).IsEqualTo("Test.Map.Gbx");
        await Assert.That((char)Downloads.Files[0].Data[TestGbx.BodyCompressionIndex]).IsEqualTo('U');
        await Assert.That(cut.Find(".status").TextContent.Trim()).StartsWith("Decompressed");
    }

    [Test]
    public async Task Upload_WhenToolFails_ShowsError()
    {
        // Arrange
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "extract-embedded-items"));

        // Act
        await UploadAsync(cut, CreateUpload("Test.Map.Gbx", TestGbx.SaveMap().ToArray()));

        // Assert
        await Assert.That(Downloads.Files).IsEmpty();
        await Assert.That(cut.Find(".error-message").TextContent.Trim()).IsEqualTo("No embedded items found.");
    }

    [Test]
    public async Task Upload_WithZip_ProcessesEachEntryIntoZip()
    {
        // Arrange
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "decompress-gbx"));

        using var zipStream = new MemoryStream();
        using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var name in new[] { "A.Map.Gbx", "B.Map.Gbx" })
            {
                using var entry = zip.CreateEntry(name).Open();
                TestGbx.SaveMap().CopyTo(entry);
            }
        }

        // Act
        await UploadAsync(cut, CreateUpload("Maps.zip", zipStream.ToArray(), "application/zip"));

        // Assert
        await Assert.That(Downloads.Files.Count).IsEqualTo(1);
        await Assert.That(Downloads.Files[0].FileName).IsEqualTo("Maps.zip");

        using var output = new ZipArchive(new MemoryStream(Downloads.Files[0].Data));
        await Assert.That(output.Entries.Select(x => x.FullName)).IsEquivalentTo(["A.Map.Gbx", "B.Map.Gbx"]);
    }

    [Test]
    public async Task Upload_KeepsLogsPerTool()
    {
        // Arrange
        var cut = Render<GbxIoComponent>(parameters => parameters.Add(x => x.Tool, "decompress-gbx"));
        await UploadAsync(cut, CreateUpload("Test.Map.Gbx", TestGbx.SaveMap().ToArray()));

        // Act
        cut.FindAll("button").Single(x => x.TextContent.Trim() == "Logs").Click();

        // Assert
        await Assert.That(cut.Find(".reu-modal, dialog, [role=dialog]").TextContent).Contains("Received file Test.Map.Gbx");
    }
}
