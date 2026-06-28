using GBX.NET;
using GBX.NET.NewtonsoftJson;
using BigBang1112.GbxTools.IO.Data;

namespace BigBang1112.GbxTools.IO.Tools;

public sealed class GbxToJsonTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, TextData>(endpoint, provider)
{
    public override string Name => "Gbx to JSON";

    public override IEnumerable<string> OutputExtensions => ["json"];

    public override Task<TextData> ProcessAsync(Gbx input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TextData(input.FilePath + ".json", input.ToJson(), "application/json"));
    }
}
