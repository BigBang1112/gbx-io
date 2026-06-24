using BigBang1112.GbxTools.IO.Attributes;
using BigBang1112.GbxTools.IO.Data;
using BigBang1112.GbxTools.IO.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace BigBang1112.GbxTools.IO.Services;

public sealed class ToolService
{
    private readonly GbxService gbxService;
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<ToolService> logger;

    public ToolService(GbxService gbxService, IServiceProvider serviceProvider, ILogger<ToolService> logger)
    {
        this.gbxService = gbxService;
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    public IoTool? GetTool(string toolId)
    {
        return serviceProvider.GetKeyedService<IoTool>(toolId);
    }

    public async Task<object?> ProcessFileAsync(string toolId, BinData data, CancellationToken cancellationToken)
    {
        var tool = serviceProvider.GetKeyedService<IoTool>(toolId);

        if (tool is null)
        {
            logger.LogWarning("Tool {ToolId} not found.", toolId);
            return null;
        }

        var toolType = tool.GetType();
        var baseType = GetIoToolBaseType(toolType);

        if (baseType is null)
        {
            logger.LogWarning("Tool {ToolId} is not an IoTool.", toolId);
            return null;
        }

        var genericArguments = baseType.GetGenericArguments();

        var inputType = genericArguments[0];
        var outputType = genericArguments[1]; // probably not needed, output can be type checked

        var headerOnly = Attribute.IsDefined(toolType.GetMethods()
            .First(m => m.Name == nameof(IoTool.ProcessAsync))
            .GetParameters()[0], typeof(HeaderOnlyAttribute));

        return await ProcessToolAsync(tool, data, inputType, headerOnly, cancellationToken);
    }

    internal static Type? GetIoToolBaseType(Type toolType)
    {
        var baseType = toolType.BaseType;

        while (baseType is not null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(IoTool<,>))
            {
                return baseType;
            }

            baseType = baseType.BaseType;
        }

        return baseType;
    }

    private async Task<object?> ProcessToolAsync(IoTool tool, BinData data, Type inputType, bool headerOnly, CancellationToken cancellationToken)
    {
        if (inputType == typeof(BinData))
        {
            return await tool.ProcessAsync(data, cancellationToken);
        }

        if (inputType == typeof(GbxData))
        {
            return await tool.ProcessAsync(new GbxData(data.FileName, data.Stream), cancellationToken);
        }

        if (inputType == typeof(TextData))
        {
            return await tool.ProcessAsync(await data.ToTextDataAsync(cancellationToken: cancellationToken), cancellationToken);
        }

        var gbx = await gbxService.ParseGbxAsync(data.Stream, headerOnly);

        if (gbx is null)
        {
            return null;
        }

        gbx.FilePath = data.FileName;
        return await tool.ProcessAsync(gbx, cancellationToken);
    }
}
