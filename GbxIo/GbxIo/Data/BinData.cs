using System.Text;

namespace GbxIo.Data;

public sealed record BinData(string? FileName, byte[] Data) : IData
{
    public TextData ToTextData() => new(FileName, Encoding.UTF8.GetString(Data), "txt");
}
