using System.Globalization;
using System.Text;

using Finnimon.Math;

namespace Finnimon.Stl;

public static class StlReader
{
    public static Stl Read(string path) => Read(File.OpenRead(path));
    public static Stl Read(Stream stream, bool leaveOpen = false)
    {
        try
        {
            var solid = new byte[5];
            var read = stream.Read(solid);
            stream.Seek(-read, SeekOrigin.Current);
            return Encoding.ASCII.GetString(solid, 0, 5).ToLower() switch
            {
                nameof(solid) => ReadAscii(stream),
                _ => ReadBinary(stream)
            };
        }
        finally
        {
            if (!leaveOpen) stream.Dispose();
        }
    }

    #region binary stl
    private const uint MaxMemorySize = 1_300_000_000;
    private const int BinaryStlHeaderLength = 84;
    private static unsafe Stl ReadBinary(Stream stream)
    {
        var remaining = stream.Length - stream.Position;
        if (remaining < BinaryStlHeaderLength)
            throw new FormatException($"Stl stream is too short.\r\nMinimum Length={BinaryStlHeaderLength}. \r\nRemaining Length={remaining}.");

        var headerBuffer = new byte[BinaryStlHeaderLength];
        stream.ReadExactly(headerBuffer, 0, BinaryStlHeaderLength);
        var header = Encoding.ASCII.GetString(headerBuffer, 0, 80);
        var triangleCount = BitConverter.ToUInt32(headerBuffer, 80);
        var remainingBodyBytes = stream.Length - stream.Position;
        var expectedBodyBytes = triangleCount * Stride;
        if (expectedBodyBytes < remainingBodyBytes)
            throw new FormatException($"Stl stream body is too short.\r\nExpected Length={expectedBodyBytes}. \r\nRemaining Length={remainingBodyBytes}.");

        var inMemoryByteSize = sizeof(StlFacet) * triangleCount;
        if (inMemoryByteSize >= MaxMemorySize)
            throw new Exception($"Stl could not be read.\n{triangleCount} triangles is too large for the runtime.");
        var triangles = new StlFacet[triangleCount];
        var buffer = new byte[Stride];
        for (uint i = 0; i < triangleCount; i++) triangles[i] = stream.ReadBinaryFacetFast(buffer);
        return new Stl(null, header, triangles);
    }

    private const int Stride = 50;
    private static unsafe StlFacet ReadBinaryFacetFast(this Stream stream, byte[] buffer)
    {
        stream.ReadExactly(buffer, 0, Stride);
        const int normalSkip = 12;
        fixed (byte* bufPtr = &buffer[normalSkip]) return ((StlFacet*)bufPtr)[0];
    }

    private static StlFacet ReadBinaryFacet(this BinaryReader binary)
    {
        binary.Skip(sizeof(float) * 3);
        Vertex3D a = new(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
        Vertex3D b = new(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
        Vertex3D c = new(binary.ReadSingle(), binary.ReadSingle(), binary.ReadSingle());
        var attribByteCount = binary.ReadUInt16();
        var triangle = new Triangle3D(a, b, c);
        return new StlFacet(triangle, attribByteCount);
    }

    private static void Skip(this BinaryReader binary, uint count) => binary.BaseStream.Seek(count, SeekOrigin.Current);

    #endregion

    #region ascii stl

    private static Stl ReadAscii(Stream stream)
    {
        using var reader = new StreamReader(stream, leaveOpen: true);
        var headerLine = reader.ReadLine()?.Trim().Split(' ') ?? [];
        var name = headerLine.Length >= 2 ? headerLine[1] : "";
        var header = headerLine.Length < 3 ? "" : string.Join(' ', headerLine[2..headerLine.Length]);
        var facetBlocks = reader.ReadLineBlocks(new string[7]);
        var facets = facetBlocks.Select(ExtractAsciiFacet);
        return new Stl(name, header, facets.ToList());
    }

    private static StlFacet ExtractAsciiFacet(string[] asciiFacet)
    {
        var a = ExtractAsciiVertex(asciiFacet[2]);
        var b = ExtractAsciiVertex(asciiFacet[3]);
        var c = ExtractAsciiVertex(asciiFacet[4]);
        const ushort defaultAttribByteCount = 0;
        var triangle = new Triangle3D(a, b, c);
        return new StlFacet(triangle, defaultAttribByteCount);
    }

    private static Vertex3D ExtractAsciiVertex(string asciiVertex)
    {
        var split = asciiVertex.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var x = float.Parse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture);
        var y = float.Parse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture);
        var z = float.Parse(split[3], NumberStyles.Float, CultureInfo.InvariantCulture);
        return new Vertex3D(x, y, z);
    }

    private static IEnumerable<string[]> ReadLineBlocks(this StreamReader reader, string[] buffer)
    {
        var pos = 0;
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            buffer[pos] = line;
            pos++;
            var blockComplete = pos >= buffer.Length;
            if (!blockComplete) continue;
            pos = 0;
            yield return buffer;
        }
    }

    #endregion
}
