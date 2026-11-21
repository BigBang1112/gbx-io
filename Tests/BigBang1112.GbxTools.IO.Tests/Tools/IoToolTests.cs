using BigBang1112.GbxTools.IO.Exceptions;
using GBX.NET.Engines.Game;
using GBX.NET.Managers;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class IoToolTests
{
    private readonly IServiceProvider provider = TestServices.Create();

    [Test]
    public async Task InputExtensions_ForGbxData_ReturnsGbx()
    {
        // Act
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");

        // Assert
        await Assert.That(tool.InputExtensions).IsEquivalentTo(["Gbx"]);
    }

    [Test]
    public async Task InputExtensions_ForPakData_ReturnsPakAndPackGbx()
    {
        // Act
        var tool = provider.GetTool<PakToZipTool>("pak-to-zip");

        // Assert
        await Assert.That(tool.InputExtensions).IsEquivalentTo(["pak", "Pack.Gbx"]);
    }

    [Test]
    public async Task OutputExtensions_ForZipData_ReturnsZip()
    {
        // Act
        var tool = provider.GetTool<ExtractEmbeddedItemsTool>("extract-embedded-items");

        // Assert
        await Assert.That(tool.OutputExtensions).IsEquivalentTo(["zip"]);
    }

    [Test]
    public async Task InputExtensions_ForTypedGbx_ReturnsExtensionsOfNodeClass()
    {
        // Act
        var tool = provider.GetTool<RemoveValidationGhostTool>("remove-validation-ghost");

        // Assert
        await Assert.That(tool.InputExtensions).IsNotEmpty();
        await Assert.That(tool.InputExtensions).IsEquivalentTo(ClassManager.GetFileExtensions(CGameCtnChallenge.Id));
    }

    [Test]
    public async Task OutputExtensions_ForEnumerableOfTypedGbx_ReturnsExtensionsOfNodeClass()
    {
        // Act
        var tool = provider.GetTool<ExtractGhostsTool>("extract-ghosts");

        // Assert
        await Assert.That(tool.OutputExtensions).IsNotEmpty();
        await Assert.That(tool.OutputExtensions).IsEquivalentTo(ClassManager.GetFileExtensions(CGameCtnGhost.Id));
    }

    [Test]
    public async Task OutputExtensions_ForEnumerableOfTextData_ReturnsTxt()
    {
        // Act
        var tool = provider.GetTool<ExtractInputsTool>("extract-inputs");

        // Assert
        await Assert.That(tool.OutputExtensions).IsEquivalentTo(["txt"]);
    }

    [Test]
    public async Task InputExtensions_ForUntypedGbx_ReturnsGbx()
    {
        // Act
        var tool = provider.GetTool<GbxToJsonTool>("gbx-to-json");

        // Assert
        await Assert.That(tool.InputExtensions).IsEquivalentTo(["Gbx"]);
    }

    [Test]
    public async Task ProcessAsync_WithUnmatchingInput_ThrowsWithReadableTypeName()
    {
        // Arrange
        var tool = provider.GetTool<RemoveValidationGhostTool>("remove-validation-ghost");

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync((object)"not a map", NullLogger.Instance, CancellationToken.None))
            .Throws<UnmatchingInputException>()
            .WithMessage("Input must be of type Gbx<CGameCtnChallenge>.");
    }

    [Test]
    public async Task ProcessAsync_WithUnmatchingNonGenericInput_ThrowsWithTypeName()
    {
        // Arrange
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync((object)"not a Gbx", NullLogger.Instance, CancellationToken.None))
            .Throws<UnmatchingInputException>()
            .WithMessage("Input must be of type GbxData.");
    }

    [Test]
    public async Task ReportAsync_ReportsMessageToProgress()
    {
        // Arrange
        var progress = new TestProgress();
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");
        tool.Progress = progress;

        // Act
        await tool.ReportAsync("Working...", NullLogger.Instance);

        // Assert
        await Assert.That(progress.Messages).IsEquivalentTo(["Working..."]);
    }

    [Test]
    public async Task ReportAsync_WhenCanceled_Throws()
    {
        // Arrange
        var tool = provider.GetTool<CompressGbxTool>("compress-gbx");

        // Act & Assert
        await Assert.That(() => tool.ReportAsync("Working...", NullLogger.Instance, new CancellationToken(canceled: true)))
            .Throws<OperationCanceledException>();
    }
}
