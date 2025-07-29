namespace GbxIo.Client.Data;

public sealed record GbxData(string? FileName, byte[] Data) : IData;
