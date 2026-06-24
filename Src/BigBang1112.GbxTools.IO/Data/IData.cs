namespace BigBang1112.GbxTools.IO.Data;

public interface IData
{
    string? FileName { get; }
    string Type { get; }
    Stream Stream { get; }
}
