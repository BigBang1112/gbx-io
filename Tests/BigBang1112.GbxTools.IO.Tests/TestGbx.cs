using GBX.NET;
using GBX.NET.Engines.Game;

namespace BigBang1112.GbxTools.IO.Tests;

/// <summary>
/// Gbx files made in memory, as there are no sample files in the repository.
/// </summary>
internal static class TestGbx
{
    /// <summary>
    /// Index of the byte in the Gbx header that says whether the body is compressed ('C') or not ('U').
    /// </summary>
    public const int BodyCompressionIndex = 7;

    public static Gbx<CGameCtnChallenge> CreateMap(string? filePath = "Test.Map.Gbx")
    {
        return new Gbx<CGameCtnChallenge>(new CGameCtnChallenge()) { FilePath = filePath };
    }

    public static MemoryStream Save(Gbx gbx)
    {
        var stream = new MemoryStream();
        gbx.Save(stream);
        stream.Position = 0;
        return stream;
    }

    public static MemoryStream SaveMap() => Save(CreateMap());

    public static async Task<MemoryStream> DecompressAsync(Stream input)
    {
        var output = new MemoryStream();
        await Gbx.DecompressAsync(input, output);
        output.Position = 0;
        return output;
    }

    public static async Task<MemoryStream> CompressAsync(Stream input)
    {
        var output = new MemoryStream();
        await Gbx.CompressAsync(input, output);
        output.Position = 0;
        return output;
    }
}
