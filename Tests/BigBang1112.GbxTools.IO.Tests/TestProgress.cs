namespace BigBang1112.GbxTools.IO.Tests;

/// <summary>
/// Reports right away, unlike <see cref="Progress{T}"/>, which posts to the synchronization context.
/// </summary>
internal sealed class TestProgress : IProgress<string>
{
    public List<string> Messages { get; } = [];

    public void Report(string value) => Messages.Add(value);
}
