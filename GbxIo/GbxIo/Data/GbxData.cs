namespace GbxIo.Data;

public sealed record GbxData(string? FileName, byte[] Data) : IData;
