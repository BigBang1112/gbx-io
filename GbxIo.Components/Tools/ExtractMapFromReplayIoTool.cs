using GBX.NET;
using GBX.NET.Engines.Game;
using GbxIo.Components.Attributes;
using TmEssentials;

namespace GbxIo.Components.Tools;

public sealed class ExtractMapFromReplayIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnReplayRecord>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Extract map from replay";

    public override async Task<Gbx<CGameCtnChallenge>> ProcessAsync([IgnoreExceptionsInBody] Gbx<CGameCtnReplayRecord> input, CancellationToken cancellationToken)
    {
        var mapGbx = await input.Node.GetChallengeAsync(cancellationToken: cancellationToken) ?? throw new InvalidOperationException("No map found.");
        var map = mapGbx.Node;

        var isManiaPlanet = map.CanBeGameVersion(
              GameVersion.MP1
            | GameVersion.MP2
            | GameVersion.MP3
            | GameVersion.TMT
            | GameVersion.MP4
            | GameVersion.TM2020);

        if (isManiaPlanet)
        {
            map.CreateChunk<CGameCtnChallenge.HeaderChunk03043003>();

            if (map.KindInHeader == CGameCtnChallenge.MapKind.EndMarker)
            {
                map.KindInHeader = map.Kind;
            }
        }

        var extension = isManiaPlanet ? ".Map.Gbx" : ".Challenge.Gbx";

        var mapName = TextFormatter.Deformat(map.MapName);

        foreach (var ch in GbxPath.InvalidFileNameChars)
        {
            mapName = mapName.Replace(ch, '_');
        }

        mapGbx.FilePath = mapName + extension;

        return mapGbx;
    }
}
