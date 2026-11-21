using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Reuniverse.Razor;

namespace BigBang1112.GbxTools.IO.Tests.Components;

public class ToolMenuTests : ComponentTestBase
{
    [Test]
    public async Task Render_ShowsButtonForEveryTool()
    {
        // Arrange
        var toolNames = Services.GetServices<IoTool>().Select(x => x.Name).ToList();

        // Act
        var cut = Render<ToolMenu>(parameters => parameters.Add(x => x.SelectedToolEndpoint, null));

        // Assert
        await Assert.That(cut.FindAll(".tool-menu button").Select(x => x.TextContent.Trim())).IsEquivalentTo(toolNames);
    }

    [Test]
    public async Task Render_WithSelectedTool_MarksItActive()
    {
        // Act
        var cut = Render<ToolMenu>(parameters => parameters.Add(x => x.SelectedToolEndpoint, "gbx-to-json"));

        // Assert
        var active = cut.FindAll(".tool-menu button.active");
        await Assert.That(active.Count).IsEqualTo(1);
        await Assert.That(active[0].TextContent.Trim()).IsEqualTo("Gbx to JSON");
        await Assert.That(cut.Instance.SelectedTool?.Endpoint).IsEqualTo("gbx-to-json");
    }

    [Test]
    public async Task Click_SelectsToolAndRaisesSelectedToolChanged()
    {
        // Arrange
        IoTool? selected = null;
        var cut = Render<ToolMenu>(parameters => parameters
            .Add(x => x.SelectedToolEndpoint, null)
            .Add(x => x.SelectedToolChanged, tool => selected = tool));

        // Act
        cut.FindAll(".tool-menu button").Single(x => x.TextContent.Trim() == "Decompress Gbx").Click();

        // Assert
        await Assert.That(selected?.Endpoint).IsEqualTo("decompress-gbx");
        await Assert.That(cut.Find(".tool-menu button.active").TextContent.Trim()).IsEqualTo("Decompress Gbx");
    }

    [Test]
    public async Task Render_WhenNotInteractive_ShowsLoader()
    {
        // Arrange
        SetRendererInfo(new RendererInfo("Static", isInteractive: false));

        // Act
        var cut = Render<ToolMenu>(parameters => parameters.Add(x => x.SelectedToolEndpoint, null));

        // Assert
        await Assert.That(cut.Find(".tool-menu").ClassList.Contains("tool-menu-loading")).IsTrue();
        await Assert.That(cut.FindComponents<ReuLoader>().Count).IsEqualTo(1);
    }

    [Test]
    public async Task Render_WhenInteractive_HidesLoader()
    {
        // Act
        var cut = Render<ToolMenu>(parameters => parameters.Add(x => x.SelectedToolEndpoint, null));

        // Assert
        await Assert.That(cut.Find(".tool-menu").ClassList.Contains("tool-menu-loading")).IsFalse();
        await Assert.That(cut.FindComponents<ReuLoader>().Count).IsEqualTo(0);
    }
}
