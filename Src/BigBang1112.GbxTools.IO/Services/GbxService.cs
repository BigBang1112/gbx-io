using GBX.NET;
using GBX.NET.Exceptions;
using GBX.NET.LZO;
using GBX.NET.ZLib;
using Microsoft.Extensions.Logging;

namespace BigBang1112.GbxTools.IO.Services;

public sealed class GbxService(ILogger<GbxService> logger)
{
    private readonly ILogger<GbxService> logger = logger;

    static GbxService()
    {
        Gbx.LZO = new Lzo();
        Gbx.ZLib = new ZLib();
    }

    public async ValueTask<Gbx?> ParseGbxAsync(Stream stream, bool headerOnly, bool ignoreExceptionsInBody, ILogger? logger)
    {
        logger ??= this.logger;

        try
        {
            return headerOnly
                ? Gbx.ParseHeader(stream, new() { Logger = logger })
                : await Gbx.ParseAsync(stream, new() { Logger = logger, IgnoreExceptionsInBody = ignoreExceptionsInBody });
        }
        catch (NotAGbxException)
        {
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to parse Gbx file.");
            throw;
        }
    }
}
