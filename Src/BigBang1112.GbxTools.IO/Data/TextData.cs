using System.Text;

namespace BigBang1112.GbxTools.IO.Data;

public sealed record TextData(string? FileName, string Text, string Type = "text/plain") : IData
{
    private Stream? stream;
    public Stream Stream => stream ??= new MemoryStream(Encoding.UTF8.GetBytes(Text));
}
