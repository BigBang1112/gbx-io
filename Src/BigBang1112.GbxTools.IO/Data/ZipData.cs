namespace BigBang1112.GbxTools.IO.Data;

public sealed record ZipData(string? FileName, Stream Stream) : IData
{
    public string Type => "application/zip";

    public ZipData(string? fileName, byte[] data) : this(fileName, new MemoryStream(data))
    {
    }
}
