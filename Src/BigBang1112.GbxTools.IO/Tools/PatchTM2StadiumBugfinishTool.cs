using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class PatchTM2StadiumBugfinishTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Patch TM² Stadium bugfinish";

    public override IEnumerable<string> InputExtensions => ["Map.Gbx"];
    public override IEnumerable<string> OutputExtensions => ["Map.Gbx"];

    public override async Task<Gbx<CGameCtnChallenge>> ProcessAsync(Gbx<CGameCtnChallenge> input, ILogger logger, CancellationToken cancellationToken)
    {
        return input;
    }
}