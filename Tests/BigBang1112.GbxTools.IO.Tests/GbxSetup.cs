using GBX.NET;
using GBX.NET.LZO;
using GBX.NET.ZLib;

namespace BigBang1112.GbxTools.IO.Tests;

public static class GbxSetup
{
    /// <summary>
    /// The app sets these up in GbxService, which not every test goes through.
    /// </summary>
    [Before(TestSession)]
    public static void SetUpCompression()
    {
        Gbx.LZO = new Lzo();
        Gbx.ZLib = new ZLib();
    }
}
