namespace GbxIo.Data;

public sealed record TextData(string? FileName, string Text, string Format) : IData;
