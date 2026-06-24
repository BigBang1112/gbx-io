using System.Text;

namespace BigBang1112.GbxTools.IO.Data;

public sealed record BinData(string? FileName, Stream Stream, string Type) : IData
{
    public async Task<TextData> ToTextDataAsync(string type = "text/plain", CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(Stream, Encoding.UTF8);
        return new(FileName, await reader.ReadToEndAsync(cancellationToken), type);
    }
}