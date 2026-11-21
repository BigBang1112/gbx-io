using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using GbxIo.Components.Data;
using System.Text;

namespace GbxIo.Components.Tools;

public class ExtractInputsIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, IEnumerable<TextData>>(endpoint, provider)
{
    public override string Name => "Extract inputs";

    protected virtual string Format => "txt";

    public override Task<IEnumerable<TextData>> ProcessAsync(Gbx input, CancellationToken cancellationToken)
	{
        var fileName = Path.GetFileName(input.FilePath);

        // TM1.0 replay inputs are in the validation ghost since GBX.NET 2.5, which GetGhosts() already returns
        var ghostInputs = input switch
        {
            Gbx<CGameCtnGhost> ghost => GetGhostInputs(ghost),
            Gbx<CGameCtnReplayRecord> replay => replay.Node.GetGhosts().SelectMany(GetGhostInputs),
            Gbx<CGameCtnMediaClip> clip => clip.Node.GetGhosts().SelectMany(GetGhostInputs),
            _ => throw new InvalidOperationException("Only Replay.Gbx, Clip.Gbx, and Ghost.Gbx is supported."),
        };

        var inputFiles = new List<TextData>();

        var i = 0;

        foreach (var inputs in ghostInputs.Where(x => x.Any()))
        {
            inputFiles.Add(new TextData($"{GbxPath.GetFileNameWithoutExtension(fileName ?? "Ghost")}_{++i:00}.txt", CreateInputText(inputs), Format));
        }

        return Task.FromResult(inputFiles.AsEnumerable());
    }

    private static IEnumerable<IEnumerable<IInput>> GetGhostInputs(CGameCtnGhost ghost)
    {
        IEnumerable<IEnumerable<IInput>> ghostInputs = [ghost.GetSimulationInputs()];

        if (ghost.PlayerInputs is not null)
        {
            return ghostInputs.Concat(ghost.PlayerInputs.Select(x => x.Inputs));
        }

        return ghostInputs;
    }

    protected virtual string CreateInputText(IEnumerable<IInput> inputs)
    {
        var sb = new StringBuilder();

        foreach (var input in inputs)
        {
            sb.AppendLine(input.ToString());
        }

        return sb.ToString();
    }
}
