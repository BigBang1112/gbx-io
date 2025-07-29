﻿using GBX.NET;
using GBX.NET.NewtonsoftJson;
using GbxIo.Client.Data;

namespace GbxIo.Client.Tools;

public sealed class GbxToJsonIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, TextData>(endpoint, provider)
{
    public override string Name => "Gbx to JSON";

    public override Task<TextData> ProcessAsync(Gbx input, CancellationToken cancellationToken)
    {
        return Task.FromResult(new TextData(input.FilePath + ".json", input.ToJson(), "json"));
    }
}
