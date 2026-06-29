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

    public async Task<object?> ProcessFileAsync(string toolId, BinData data, ILogger? logger, CancellationToken cancellationToken)
    {
        logger ??= this.logger;

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

        var gbxParameter = toolType.GetMethods()
            .First(m => m.Name == nameof(IoTool.ProcessAsync))
            .GetParameters()[0];

        var headerOnly = Attribute.IsDefined(gbxParameter, typeof(HeaderOnlyAttribute));
        var ignoreExceptionsInBody = Attribute.IsDefined(gbxParameter, typeof(IgnoreExceptionsInBodyAttribute));

        return await ProcessToolAsync(tool, data, inputType, headerOnly, ignoreExceptionsInBody, logger, cancellationToken);
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

    private async Task<object?> ProcessToolAsync(IoTool tool, BinData data, Type inputType, bool headerOnly, bool ignoreExceptionsInBody, ILogger logger, CancellationToken cancellationToken)
    {
        if (inputType == typeof(BinData))
        {
            return await tool.ProcessAsync(data, logger, cancellationToken);
        }

        if (inputType == typeof(GbxData))
        {
            return await tool.ProcessAsync(new GbxData(data.FileName, data.Stream), logger, cancellationToken);
        }

        if (inputType == typeof(PakData))
        {
            return await tool.ProcessAsync(new PakData(data.FileName, data.Stream), logger, cancellationToken);
        }

        if (inputType == typeof(ZipData))
        {
            return await tool.ProcessAsync(new ZipData(data.FileName, data.Stream), logger, cancellationToken);
        }

        if (inputType == typeof(TextData))
        {
            return await tool.ProcessAsync(await data.ToTextDataAsync(cancellationToken: cancellationToken), logger, cancellationToken);
        }

        var gbx = await gbxService.ParseGbxAsync(data.Stream, headerOnly, ignoreExceptionsInBody, logger);

        if (gbx is null)
        {
            return null;
        }

        if (gbx.Body.Exception is not null)
        {
            logger.LogWarning(gbx.Body.Exception, "Gbx has exceptions in body, but will be processed anyway.");
        }

        gbx.FilePath = data.FileName;
        return await tool.ProcessAsync(gbx, logger, cancellationToken);
    }
}
