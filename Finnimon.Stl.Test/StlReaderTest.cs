using System.Diagnostics;

namespace Finnimon.Stl.Test;

public class StlReaderTest
{
    private static IEnumerable<string> FilePaths => Directory.GetFiles("Files", "*.stl");

    [TestCaseSource(nameof(FilePaths))]
    public void TestRead(string file)
    {
        using var stream = File.OpenRead(file);
        var sw = Stopwatch.StartNew();
        var stl = StlReader.Read(stream, leaveOpen: false);
        Console.WriteLine($"Duration: {sw.Elapsed.TotalMilliseconds}ms");
        Assert.That(stl, Is.Not.Null);
    }
}