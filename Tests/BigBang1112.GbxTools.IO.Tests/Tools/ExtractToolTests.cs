using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using System.Text.Json;
using TmEssentials;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class ExtractToolTests
{
    private readonly IServiceProvider provider = TestServices.Create();

    private static TimeInt32 Ms(int milliseconds) => TimeInt32.FromMilliseconds(milliseconds);

    private static Gbx<CGameCtnGhost> CreateGhost(params IInput[] inputs)
    {
        return new Gbx<CGameCtnGhost>(new CGameCtnGhost { Inputs = [.. inputs] }) { FilePath = "Test.Ghost.Gbx" };
    }

    [Test]
    public async Task ExtractGhosts_FromMap_ReturnsValidationGhost()
    {
        // Arrange
        var tool = provider.GetTool<ExtractGhostsTool>("extract-ghosts");
        var ghost = new CGameCtnGhost();
        var map = TestGbx.CreateMap();
        map.Node.ChallengeParameters = new CGameCtnChallengeParameters { RaceValidateGhost = ghost };

        // Act
        var output = (await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None)).ToList();

        // Assert
        await Assert.That(output.Count).IsEqualTo(1);
        await Assert.That(output[0].Node).IsSameReferenceAs(ghost);
        await Assert.That(output[0].FilePath).IsEqualTo("Test_01.Ghost.Gbx");
    }

    [Test]
    public async Task ExtractGhosts_FromMapWithoutGhosts_ReturnsNothing()
    {
        // Arrange
        var tool = provider.GetTool<ExtractGhostsTool>("extract-ghosts");

        // Act
        var output = await tool.ProcessAsync(TestGbx.CreateMap(), NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsEmpty();
    }

    [Test]
    public async Task ExtractGhosts_FromUnsupportedGbx_Throws()
    {
        // Arrange
        var tool = provider.GetTool<ExtractGhostsTool>("extract-ghosts");

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(CreateGhost(), NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExtractInputs_FromGhost_ReturnsInputPerLine()
    {
        // Arrange
        var tool = provider.GetTool<ExtractInputsTool>("extract-inputs");
        var accelerate = new Accelerate(Ms(100), true);
        var steer = new Steer(Ms(200), -65536);

        // Act
        var output = (await tool.ProcessAsync(CreateGhost(accelerate, steer), NullLogger.Instance, CancellationToken.None)).ToList();

        // Assert
        await Assert.That(output.Count).IsEqualTo(1);
        await Assert.That(output[0].FileName).IsEqualTo("Test_01.txt");
        await Assert.That(output[0].Text.ReplaceLineEndings("\n")).IsEqualTo($"{accelerate}\n{steer}\n");
    }

    [Test]
    public async Task ExtractInputs_FromGhostWithoutInputs_ReturnsNothing()
    {
        // Arrange
        var tool = provider.GetTool<ExtractInputsTool>("extract-inputs");

        // Act
        var output = await tool.ProcessAsync(CreateGhost(), NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsEmpty();
    }

    [Test]
    public async Task ExtractInputs_FromUnsupportedGbx_Throws()
    {
        // Arrange
        var tool = provider.GetTool<ExtractInputsTool>("extract-inputs");

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(TestGbx.CreateMap(), NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExtractInputsTmi_WritesTimesRelativeToRaceStart()
    {
        // Arrange
        var tool = provider.GetTool<ExtractInputsTmiTool>("extract-inputs-tmi");
        var ghost = CreateGhost(
            new FakeIsRaceRunning(Ms(1000), true),
            new Accelerate(Ms(1100), true),
            new Steer(Ms(1200), -65536),
            new SteerLeft(Ms(1300), true),
            new Accelerate(Ms(1500), false),
            new Respawn(Ms(1605), true),
            new Respawn(Ms(1700), false));

        // Act
        var output = (await tool.ProcessAsync(ghost, NullLogger.Instance, CancellationToken.None)).Single();

        // Assert
        await Assert.That(output.Text.ReplaceLineEndings("\n")).IsEqualTo("""
            #1000 fakeIsRaceRunning
            90 press up
            190 steer -65536
            290 press left
            490 rel up
            600 press enter
            #1690 rel enter

            """.ReplaceLineEndings("\n"));
    }

    [Test]
    public async Task ExtractInputsTmi_AfterDontInverseAxis_InvertsSteering()
    {
        // Arrange
        var tool = provider.GetTool<ExtractInputsTmiTool>("extract-inputs-tmi");
        var ghost = CreateGhost(
            new FakeDontInverseAxis(Ms(0), true),
            new Steer(Ms(100), 1000));

        // Act
        var output = (await tool.ProcessAsync(ghost, NullLogger.Instance, CancellationToken.None)).Single();

        // Assert
        await Assert.That(output.Text.ReplaceLineEndings("\n")).IsEqualTo("""
            #-10 fakeDontInverseAxis
            90 steer -1000

            """.ReplaceLineEndings("\n"));
    }

    [Test]
    public async Task GbxToJson_ReturnsJsonNamedAfterGbx()
    {
        // Arrange
        var tool = provider.GetTool<GbxToJsonTool>("gbx-to-json");

        // Act
        var output = await tool.ProcessAsync(TestGbx.CreateMap(), NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.FileName).IsEqualTo("Test.Map.Gbx.json");
        await Assert.That(output.Type).IsEqualTo("application/json");
        await Assert.That(() => JsonDocument.Parse(output.Text)).ThrowsNothing();
    }

    [Test]
    public async Task ExtractMesh_FromUnsupportedGbx_Throws()
    {
        // Arrange
        var tool = provider.GetTool<ExtractMeshTool>("extract-mesh");

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(TestGbx.CreateMap(), NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>();
    }

    [Test]
    public async Task ExtractThumbnail_FromMapWithoutThumbnail_ReturnsNull()
    {
        // Arrange
        var tool = provider.GetTool<ExtractThumbnailTool>("extract-thumbnail");

        // Act
        var output = await tool.ProcessAsync(TestGbx.CreateMap(), NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsNull();
    }

    [Test]
    public async Task PakToZip_WithUnknownPak_Throws()
    {
        // Arrange
        var tool = provider.GetTool<PakToZipTool>("pak-to-zip");
        var input = new PakData("Unknown.pak", new MemoryStream([1, 2, 3]));

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(input, NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("No valid key found for Unknown.pak");
    }
}
