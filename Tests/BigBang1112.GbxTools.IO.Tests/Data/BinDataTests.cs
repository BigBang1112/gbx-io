using System.Text;

namespace BigBang1112.GbxTools.IO.Tests.Data;

public class BinDataTests
{
    [Test]
    public async Task ToTextDataAsync_ReadsStreamAsUtf8()
    {
        // Arrange
        var data = new BinData("file.txt", new MemoryStream(Encoding.UTF8.GetBytes("ěščřžýáíé")), "application/octet-stream");

        // Act
        var text = await data.ToTextDataAsync();

        // Assert
        await Assert.That(text.FileName).IsEqualTo("file.txt");
        await Assert.That(text.Text).IsEqualTo("ěščřžýáíé");
        await Assert.That(text.Type).IsEqualTo("text/plain");
    }

    [Test]
    public async Task ToTextDataAsync_WithType_UsesType()
    {
        // Arrange
        var data = new BinData("file.json", new MemoryStream("{}"u8.ToArray()), "application/octet-stream");

        // Act
        var text = await data.ToTextDataAsync("application/json");

        // Assert
        await Assert.That(text.Type).IsEqualTo("application/json");
    }
}
