using System.Text;
using Finnimon.Math;

namespace Finnimon.Stl;

public static class StlWriter
{
    public static bool Write(Stl stl, Stream stream, StlFileFormat format,bool leaveOpen = false)
        => format switch
        {
            StlFileFormat.Binary =>WriteBinary(stl, stream,leaveOpen),
            StlFileFormat.Ascii =>WriteAscii(stl, stream,leaveOpen),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

    private static bool WriteAscii(Stl stl, Stream stream, bool leaveOpen)
    {
        using var streamWriter = new StreamWriter(stream,Encoding.ASCII,bufferSize: 4096, leaveOpen: leaveOpen);
        var header = $"solid {stl.Name ?? ""} {stl.Header}";
        streamWriter.WriteLine(header);
        foreach (var facet in stl.Facets) streamWriter.WriteLine(FormatFacetAscii(facet));
        var footer = $"endsolid {stl.Name ?? ""}";
        streamWriter.WriteLine(footer);
        return true;
    }

    private static string FormatFacetAscii(in StlFacet facet)
    {
        var normal=facet.Triangle.Normal;
        var (a, b, c) = facet.Triangle;
        return $"""
                 facet normal {normal.X:e6} {normal.Y:e6} {normal.Z:e6}
                  outer loop
                   vertex {a.X:e6} {a.Y:e6} {a.Z:e6}
                   vertex {b.X:e6} {b.Y:e6} {b.Z:e6}
                   vertex {c.X:e6} {c.Y:e6} {c.Z:e6}
                  endloop
                 endfacet
                """;
    }

    private static bool WriteBinary(Stl stl, Stream stream, bool leaveOpen)
    {
        var name = stl.Name is null ? "" : stl.Name + " ";
        var combinedHeader = name + stl.Header;
        if(combinedHeader.StartsWith("solid",StringComparison.OrdinalIgnoreCase)) combinedHeader="_"+combinedHeader;
        var headerBytes=Encoding.ASCII.GetBytes(combinedHeader);
        var headerBuffer=new byte[80];
        for (var i = 0; i < headerBuffer.Length&&i<headerBytes.Length; i++) headerBuffer[i]=headerBytes[i];
        using var binary=new BinaryWriter(stream,Encoding.ASCII,leaveOpen: leaveOpen);
        binary.Write(headerBuffer);
        binary.Write((uint) stl.Facets.Count);
        foreach (var facet in stl.Facets)
        {
            ReadOnlySpan<float> normal = facet.Triangle.Normal;
            foreach (var f in normal) binary.Write(f);
            ReadOnlySpan<float> floats = facet.Triangle;
            foreach (var f in floats) binary.Write(f);
            binary.Write(facet.Attribute);
        }
        return true;
    }
}

public enum StlFileFormat
{
    Binary=0,
    Ascii=1
}