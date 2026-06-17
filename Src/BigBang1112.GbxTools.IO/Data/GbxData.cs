namespace BigBang1112.GbxTools.IO.Data;

public sealed record GbxData(string? FileName, byte[] Data) : IData;
