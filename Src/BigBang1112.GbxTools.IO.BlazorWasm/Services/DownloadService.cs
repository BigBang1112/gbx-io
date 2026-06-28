using BigBang1112.GbxTools.IO.Services;
using Microsoft.JSInterop;

namespace BigBang1112.GbxTools.IO.BlazorWasm.Services;

public sealed class DownloadService(IJSRuntime js) : IDownloadService
{
    private readonly IJSRuntime js = js;

    public async Task DownloadAsync(string? fileName, Stream stream)
    {
        using var streamRef = new DotNetStreamReference(stream);
        await js.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }
}
