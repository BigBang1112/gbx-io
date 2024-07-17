﻿using GBX.NET;
using GbxIo.Components.Data;

namespace GbxIo.Components.Tools;

public sealed class ExtractInputsIoTool(string endpoint, IServiceProvider provider)
    : IoTool<Gbx, TextData>(endpoint, provider)
{
    public override string Name => "Extract inputs";

    public override Task<TextData> ProcessAsync(Gbx input)
    {
        throw new NotImplementedException();
    }
}