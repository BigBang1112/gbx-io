using GBX.NET.Engines.Game;

namespace BigBang1112.GbxTools.IO.Tests.Tools;

public class MapToolTests
{
    private const int OldWoodPhysics = 7;

    private readonly IServiceProvider provider = TestServices.Create();

    [Test]
    public async Task ChangeToOldWoodPhysics_WithoutChunk_AddsChunkWithOldWoodPhysics()
    {
        // Arrange
        var tool = provider.GetTool<ChangeToOldWoodPhysicsTool>("change-to-old-wood-physics");
        var map = TestGbx.CreateMap();

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.Node.Chunks.Get<CGameCtnChallenge.Chunk03043022>()?.U01).IsEqualTo(OldWoodPhysics);
    }

    [Test]
    public async Task ChangeToOldWoodPhysics_WithNewPhysics_ChangesToOldWoodPhysics()
    {
        // Arrange
        var tool = provider.GetTool<ChangeToOldWoodPhysicsTool>("change-to-old-wood-physics");
        var map = TestGbx.CreateMap();
        map.Node.CreateChunk<CGameCtnChallenge.Chunk03043022>().U01 = 8;

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.Node.Chunks.Get<CGameCtnChallenge.Chunk03043022>()?.U01).IsEqualTo(OldWoodPhysics);
    }

    [Test]
    public async Task ChangeToOldWoodPhysics_GeneratesNewMapUid()
    {
        // Arrange
        var tool = provider.GetTool<ChangeToOldWoodPhysicsTool>("change-to-old-wood-physics");
        var map = TestGbx.CreateMap();
        map.Node.MapUid = "OriginalUid";

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.Node.MapUid).IsNotEqualTo("OriginalUid");
    }

    [Test]
    public async Task ChangeToOldWoodPhysics_WithOldWoodPhysics_Throws()
    {
        // Arrange
        var tool = provider.GetTool<ChangeToOldWoodPhysicsTool>("change-to-old-wood-physics");
        var map = TestGbx.CreateMap();
        map.Node.CreateChunk<CGameCtnChallenge.Chunk03043022>().U01 = OldWoodPhysics;

        // Act & Assert
        await Assert.That(() => tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None))
            .Throws<InvalidOperationException>()
            .WithMessage("Already using old wood physics.");
    }

    [Test]
    public async Task RemoveValidationGhost_RemovesGhost()
    {
        // Arrange
        var tool = provider.GetTool<RemoveValidationGhostTool>("remove-validation-ghost");
        var map = TestGbx.CreateMap();
        map.Node.ChallengeParameters = new CGameCtnChallengeParameters { RaceValidateGhost = new CGameCtnGhost() };

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.Node.ChallengeParameters!.RaceValidateGhost).IsNull();
    }

    [Test]
    public async Task RemoveValidationGhost_WithoutParameters_KeepsMap()
    {
        // Arrange
        var tool = provider.GetTool<RemoveValidationGhostTool>("remove-validation-ghost");
        var map = TestGbx.CreateMap();

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output).IsSameReferenceAs(map);
    }

    [Test]
    public async Task ValidateWithoutLightmaps_ReplacesLightmapsWithEmptyFrame()
    {
        // Arrange
        var tool = provider.GetTool<ValidateWithoutLightmapsTool>("validate-without-lightmaps");
        var map = TestGbx.CreateMap();
        map.Node.HasLightmaps = true;

        // Act
        var output = await tool.ProcessAsync(map, NullLogger.Instance, CancellationToken.None);

        // Assert
        await Assert.That(output.Node.HasLightmaps).IsFalse();
        await Assert.That(output.Node.LightmapFrames!.Length).IsEqualTo(1);
        await Assert.That(output.Node.LightmapFrames[0].Version).IsEqualTo(6);
    }
}
