using BigBang1112.GbxTools.IO.Data;
using BigBang1112.GbxTools.IO.Exceptions;
using GBX.NET;
using GBX.NET.Attributes;
using GBX.NET.Managers;
using System.Reflection;

namespace BigBang1112.GbxTools.IO.Tools;

public abstract class IoTool<TInput, TOutput>(string endpoint, IServiceProvider provider)
    : IoTool(endpoint, provider)
{
    private static readonly Dictionary<Type, IEnumerable<string>> extensions = new()
    {
        { typeof(ZipData), new[] { "zip" } },
        { typeof(PakData), new[] { "pak", "Pack.Gbx" } },
        { typeof(GbxData), new[] { "Gbx" } },
        { typeof(TextData), new[] { "txt" } },
        { typeof(Gbx), new[] { "Gbx" } },
    };

    public override IEnumerable<string> InputExtensions => GetExtensions(typeof(TInput));
    public override IEnumerable<string> OutputExtensions => GetExtensions(typeof(TOutput));

    private static IEnumerable<string> GetExtensions(Type type)
    {
        if (extensions.TryGetValue(type, out var exts))
        {
            return exts;
        }

        var classAttribute = type.GetCustomAttribute<ClassAttribute>();

        if (classAttribute is null)
        {
            if (type.IsGenericType)
            {
                var typeDefinition = type.GetGenericTypeDefinition();

                if (typeDefinition == typeof(Gbx<>))
                {
                    classAttribute = type.GenericTypeArguments[0].GetCustomAttribute<ClassAttribute>();
                }
                else if (typeDefinition == typeof(IEnumerable<>))
                {
                    return GetExtensions(type.GenericTypeArguments[0]);
                }
            }
        }

        if (classAttribute is null)
        {
            return [];
        }

        return ClassManager.GetFileExtensions(classAttribute.Id);
    }

    public abstract Task<TOutput> ProcessAsync(TInput input, CancellationToken cancellationToken);

    public override async Task<object?> ProcessAsync(object input, CancellationToken cancellationToken)
    {
        if (input is TInput typedInput)
        {
            return await ProcessAsync(typedInput, cancellationToken);
        }

        var type = typeof(TInput);
        var name = type.Name;

        if (type.IsGenericType)
        {
            name = $"{type.Name[..type.Name.IndexOf('`')]}<{string.Join(", ", type.GenericTypeArguments.Select(t => t.Name))}>";
        }

        throw new UnmatchingInputException($"Input must be of type {name}.");
    }
}

public abstract class IoTool(string endpoint, IServiceProvider provider)
{
    public abstract string Name { get; }
    public string Endpoint { get; } = endpoint;
    public IServiceProvider Provider { get; } = provider;

    public IProgress<string>? Progress { get; protected internal set; }

    public abstract IEnumerable<string> InputExtensions { get; }
    public abstract IEnumerable<string> OutputExtensions { get; }

    public abstract Task<object?> ProcessAsync(object input, CancellationToken cancellationToken);

    public async Task ReportAsync(string message, CancellationToken cancellationToken = default)
    {
        await Task.Delay(10, cancellationToken);
        Progress?.Report(message);
    }
}