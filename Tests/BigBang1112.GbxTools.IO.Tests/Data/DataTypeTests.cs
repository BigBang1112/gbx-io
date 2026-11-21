namespace BigBang1112.GbxTools.IO.Tests.Data;

public class DataTypeTests
{
    [Test]
    public async Task GbxData_HasGbxType()
    {
        await Assert.That(new GbxData("file.Gbx", Stream.Null).Type).IsEqualTo("application/x-gbx");
    }

    [Test]
    public async Task PakData_HasPakType()
    {
        await Assert.That(new PakData("file.pak", Stream.Null).Type).IsEqualTo("application/x-pak");
    }

    [Test]
    public async Task ZipData_HasZipType()
    {
        await Assert.That(new ZipData("file.zip", Stream.Null).Type).IsEqualTo("application/zip");
    }

    [Test]
    public async Task ZipData_FromBytes_WrapsBytesInStream()
    {
        // Act
        var data = new ZipData("file.zip", [1, 2, 3]);

        // Assert
        var ms = new MemoryStream();
        await data.Stream.CopyToAsync(ms);
        await Assert.That(ms.ToArray()).IsEquivalentTo(new byte[] { 1, 2, 3 });
    }
}
