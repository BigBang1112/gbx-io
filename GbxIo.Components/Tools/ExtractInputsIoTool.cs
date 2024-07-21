﻿using GBX.NET;
using GBX.NET.Engines.Game;
using GBX.NET.Inputs;
using GbxIo.Components.Data;
using System.Text;

namespace GbxIo.Components.Tools;

public class ExtractInputsIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, IEnumerable<TextData>>(endpoint, provider)
{
    public override string Name => "Extract inputs";

    public override Task<IEnumerable<TextData>> ProcessAsync(Gbx input)
    {
        string? fileName;
        IEnumerable<IInput> replayInputs;
        IEnumerable<IEnumerable<IInput>> ghostInputs;

        switch (input)
        {
            case Gbx<CGameCtnGhost> ghost:
                fileName = Path.GetFileName(ghost.FilePath);
                replayInputs = [];
                ghostInputs = GetGhostInputs(ghost);
                break;
            case Gbx<CGameCtnReplayRecord> replay:
                fileName = Path.GetFileName(replay.FilePath);
                replayInputs = replay.Node.Inputs?.AsEnumerable() ?? [];
                ghostInputs = replay.Node.GetGhosts().SelectMany(GetGhostInputs);
                break;
            case Gbx<CGameCtnMediaClip> clip:
                fileName = Path.GetFileName(clip.FilePath);
                replayInputs = [];
                ghostInputs = clip.Node.GetGhosts().SelectMany(GetGhostInputs);
                break;
            default:
                throw new InvalidOperationException("Only Replay.Gbx, Clip.Gbx, and Ghost.Gbx is supported.");
        }

        var inputFiles = new List<TextData>();

        if (replayInputs.Any())
        {
            inputFiles.Add(new TextData("Replay.txt", CreateInputText(replayInputs)));
        }

        var i = 0;

        foreach (var inputs in ghostInputs)
        {
            inputFiles.Add(new TextData(Path.Combine($"{GbxPath.GetFileNameWithoutExtension(fileName ?? "Ghost")}_{++i:00}.txt"), CreateInputText(inputs)));
        }

        return Task.FromResult(inputFiles.AsEnumerable());
    }

    private static IEnumerable<IEnumerable<IInput>> GetGhostInputs(CGameCtnGhost ghost)
    {
        IEnumerable<IEnumerable<IInput>> ghostInputs = [ghost.Inputs];

        if (ghost.PlayerInputs is not null)
        {
            return ghostInputs.Concat(ghost.PlayerInputs.Select(x => x.Inputs));
        }

        return ghostInputs;
    }

    public virtual string CreateInputText(IEnumerable<IInput> inputs)
    {
        var sb = new StringBuilder();

        foreach (var input in inputs)
        {
            sb.AppendLine(input.ToString());
        }

        return sb.ToString();
    }
}
