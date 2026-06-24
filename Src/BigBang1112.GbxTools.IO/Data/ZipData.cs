namespace BigBang1112.GbxTools.IO.Data;

public sealed record ZipData(string? FileName, byte[] Data) : IData
{
    public string Type => "application/zip";
}
