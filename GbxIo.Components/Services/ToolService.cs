﻿using GbxIo.Components.Attributes;
using GbxIo.Components.Data;
using GbxIo.Components.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO.Compression;

namespace GbxIo.Components.Services;

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

    public async Task<IEnumerable<object>> ProcessFileAsync(string toolId, BinData data)
    {
        var tool = serviceProvider.GetKeyedService<IoTool>(toolId);

        if (tool is null)
        {
            logger.LogWarning("Tool {ToolId} not found.", toolId);
            return [];
        }

        var toolType = tool.GetType();
        var baseType = GetIoToolBaseType(toolType);

        if (baseType is null)
        {
            logger.LogWarning("Tool {ToolId} is not an IoTool.", toolId);
            return [];
        }

        var genericArguments = baseType.GetGenericArguments();

        var inputType = genericArguments[0];
        var outputType = genericArguments[1]; // probably not needed, output can be type checked

        var headerOnly = Attribute.IsDefined(toolType.GetMethods()
            .First(m => m.Name == nameof(IoTool.ProcessAsync))
            .GetParameters()[0], typeof(HeaderOnlyAttribute));

        return await ProcessToolAsync(tool, data, inputType, headerOnly);
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

    private async Task<IEnumerable<object>> ProcessToolAsync(IoTool tool, BinData data, Type inputType, bool headerOnly)
    {
        if (inputType == typeof(BinData))
        {
            return await ProcessBinDataAsync(tool, data);
        }

        if (inputType == typeof(GbxData))
        {
            return await ProcessGbxDataAsync(tool, data);
        }

        if (inputType == typeof(TextData))
        {
            return await ProcessTextDataAsync(tool, data);
        }

        return await ProcessSpecificGbxDataAsync(tool, data, headerOnly);
    }

    private static async Task<IEnumerable<object>> ProcessBinDataAsync(IoTool tool, BinData data)
    {
        var output = await tool.ProcessAsync(data);
        return output is null ? [] : [output];
    }

    private static async Task<IEnumerable<object>> ProcessTextDataAsync(IoTool tool, BinData data)
    {
        var output = await tool.ProcessAsync(data.ToTextData());
        return output is null ? [] : [output];
    }

    private async Task<IEnumerable<object>> ProcessGbxDataAsync(IoTool tool, BinData data)
    {
        if (data.Data.Length < 4)
        {
            logger.LogWarning("Invalid GBX data.");
            return [];
        }

        if (data.Data[0] == 'G' && data.Data[1] == 'B' && data.Data[2] == 'X')
        {
            var gbxData = new GbxData(data.FileName, data.Data);
            var output = await tool.ProcessAsync(gbxData);
            return output is null ? [] : [output];
        }

        var outputs = new List<object>();

        await using var zipMs = new MemoryStream(data.Data);

        try
        {
            using var zip = new ZipArchive(zipMs, ZipArchiveMode.Read);

            foreach (var entry in zip.Entries)
            {
                if (string.IsNullOrEmpty(entry.Name))
                {
                    continue;
                }

                await using var entryStream = entry.Open();

                if (entry.Length < 4)
                {
                    logger.LogWarning("Invalid GBX data.");
                    continue;
                }

                var entryMagicData = new byte[4];
                await entryStream.ReadAsync(entryMagicData);

                if (entryMagicData[0] != 'G' || entryMagicData[1] != 'B' || entryMagicData[2] != 'X')
                {
                    logger.LogWarning("Invalid GBX data.");
                    continue;
                }

                await using var ms = new MemoryStream();
                await ms.WriteAsync(entryMagicData);
                await entryStream.CopyToAsync(ms);

                var gbxData = new GbxData(entry.FullName, ms.ToArray());
                var output = await tool.ProcessAsync(gbxData);

                if (output is not null)
                {
                    outputs.Add(output);
                }

                tool.Result = null;
            }
        }
        catch (InvalidDataException)
        {
            logger.LogWarning("Invalid GBX data.");
        }

        return outputs;
    }

    private async Task<IEnumerable<object>> ProcessSpecificGbxDataAsync(IoTool tool, BinData data, bool headerOnly)
    {
        await using var ms = new MemoryStream(data.Data);

        var gbx = await gbxService.ParseGbxAsync(ms, headerOnly);

        if (gbx is not null)
        {
            gbx.FilePath = data.FileName;
            var output = await tool.ProcessAsync(gbx);
            return output is null ? [] : [output];
        }

        var outputs = new List<object>();

        using var zipMs = new MemoryStream(data.Data);

        try
        {
            using var zip = new ZipArchive(zipMs, ZipArchiveMode.Read);

            foreach (var entry in zip.Entries)
            {
                await using var entryStream = entry.Open();

                var entryGbx = await gbxService.ParseGbxAsync(entryStream, headerOnly);

                if (entryGbx is null)
                {
                    continue;
                }

                entryGbx.FilePath = entry.Name;

                var output = await tool.ProcessAsync(entryGbx);

                if (output is not null)
                {
                    outputs.Add(output);
                }
            }
        }
        catch (InvalidDataException)
        {
            logger.LogWarning("Invalid GBX data.");
        }

        return outputs;
    }
}
