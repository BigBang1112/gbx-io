using System.Text;

namespace BigBang1112.GbxTools.IO.Data;

public sealed record BinData(string? FileName, byte[] Data, string Type) : IData
{
    public TextData ToTextData(string type = "text/plain") => new(FileName, Encoding.UTF8.GetString(Data), type);
}
