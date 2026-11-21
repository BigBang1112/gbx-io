using System.Text;

namespace BigBang1112.GbxTools.IO.Tests.Data;

public class TextDataTests
{
    [Test]
    public async Task Stream_ReturnsTextAsUtf8()
    {
        // Arrange
        var data = new TextData("file.txt", "žluťoučký kůň");

        // Act
        using var reader = new StreamReader(data.Stream, Encoding.UTF8);

        // Assert
        await Assert.That(await reader.ReadToEndAsync()).IsEqualTo("žluťoučký kůň");
    }

    [Test]
    public async Task Stream_ReturnsSameStreamEachTime()
    {
        // Arrange
        var data = new TextData("file.txt", "text");

        // Act & Assert
        await Assert.That(data.Stream).IsSameReferenceAs(data.Stream);
    }

    [Test]
    public async Task Type_DefaultsToPlainText()
    {
        // Act
        var data = new TextData("file.txt", "text");

        // Assert
        await Assert.That(data.Type).IsEqualTo("text/plain");
    }
}
