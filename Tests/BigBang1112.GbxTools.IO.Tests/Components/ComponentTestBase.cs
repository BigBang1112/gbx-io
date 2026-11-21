using BigBang1112.GbxTools.IO.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

namespace BigBang1112.GbxTools.IO.Tests.Components;

public abstract class ComponentTestBase : BunitContext
{
    protected ComponentTestBase()
    {
        // Before anything that resolves services, as they cannot be added after
        Services.AddLogging();
        Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri("http://gbx-io.test/") });
        Services.AddGbxIo();
        Services.AddSingleton<IDownloadService>(Downloads);

        JSInterop.Mode = JSRuntimeMode.Loose;
        JSInterop.SetupModule("./_content/Reuniverse.Razor/ReuUploadScreen.razor.js");
        JSInterop.SetupModule("./_content/Reuniverse.Razor/ReuUploadButton.razor.js");
        JSInterop.SetupModule("./_content/Reuniverse.Razor/ReuLogs.razor.js");
        SetRendererInfo(new RendererInfo("WebAssembly", isInteractive: true));
    }

    protected TestDownloadService Downloads { get; } = new();
}

public sealed class TestDownloadService : IDownloadService
{
    public List<(string? FileName, byte[] Data)> Files { get; } = [];

    public async Task DownloadAsync(string? fileName, Stream stream)
    {
        var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        Files.Add((fileName, ms.ToArray()));
    }
}
