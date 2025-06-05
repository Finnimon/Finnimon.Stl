namespace Finnimon.Stl.Test;

public class StlReadWriteTest
{
    [TestCase("Files/ascii-cube.stl",StlFileFormat.Ascii)]
    [TestCase("Files/ascii-cube.stl",StlFileFormat.Binary)]
    [TestCase("Files/Cube_3d_printing_sample.stl",StlFileFormat.Ascii)]
    [TestCase("Files/Cube_3d_printing_sample.stl",StlFileFormat.Binary)]
    public void ReadWriteReadEquiv(string stlFile, StlFileFormat writeMode)
    {
        var stl= StlReader.Read(File.OpenRead(stlFile));
        using var mem = new MemoryStream(1024);
        StlWriter.Write(stl,mem, writeMode,leaveOpen: true);
        mem.Position = 0;
        var postStl = StlReader.Read(mem,leaveOpen:true);
        Assert.That(postStl.Facets,Is.EquivalentTo(stl.Facets));
    }
}