using GBX.NET;
using GBX.NET.Engines.Game;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class RemoveValidationGhostTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx<CGameCtnChallenge>, Gbx<CGameCtnChallenge>>(endpoint, provider)
{
    public override string Name => "Remove validation ghost";

    public override Task<Gbx<CGameCtnChallenge>> ProcessAsync(Gbx<CGameCtnChallenge> input, ILogger logger, CancellationToken cancellationToken)
    {
        var output = input;
        output.Node.ChallengeParameters?.RaceValidateGhost = null;
        return Task.FromResult(output);
    }
}
