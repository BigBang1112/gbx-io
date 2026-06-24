namespace BigBang1112.GbxTools.IO.Data;

public sealed record GbxData(string? FileName, Stream Stream) : IData
{
    public string Type => "application/x-gbx";
}