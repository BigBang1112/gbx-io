namespace BigBang1112.GbxTools.IO.Services;

public interface IDownloadService
{
    Task DownloadAsync(string? fileName, Stream stream);
}