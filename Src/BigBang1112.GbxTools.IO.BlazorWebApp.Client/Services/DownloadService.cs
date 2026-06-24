using BigBang1112.GbxTools.IO.Services;
using Microsoft.JSInterop;

namespace BigBang1112.GbxTools.IO.BlazorWebApp.Client.Services;

public sealed class DownloadService : IDownloadService
{
    private readonly IJSRuntime js;

    public DownloadService(IJSRuntime js)
    {
        this.js = js;
    }

    public async Task DownloadAsync(string? fileName, Stream stream)
    {
        using var streamRef = new DotNetStreamReference(stream);
        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
