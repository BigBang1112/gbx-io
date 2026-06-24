namespace BigBang1112.GbxTools.IO.Data;

public sealed record PakData(string? FileName, byte[] Data) : IData
{
    public string Type => "application/x-pak";
}
