using System.Text;

namespace BigBang1112.GbxTools.IO.Data;

public sealed record TextData(string? FileName, string Text, string Type = "text/plain") : IData
{
    public byte[] Data { get; } = Encoding.UTF8.GetBytes(Text);

    public BinData ToBinData() => new(FileName, Data, Type);
}
