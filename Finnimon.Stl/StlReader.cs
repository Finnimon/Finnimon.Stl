using Finnimon.Math;

namespace Finnimon.Stl;

public static class StlReader
{
    public static Stl Read(Stream stream, bool leaveOpen = false)
    {
        var solid = new byte[5];
        var read=stream.Read(solid);
        stream.Seek(-read, SeekOrigin.Current);
        var stl= BitConverter.ToString(solid) switch
        {
            nameof(solid) => ReadAscii(stream),
            _ => ReadBinary(stream)
        };
        if(leaveOpen) return stl;
        stream.Close();
        return stl;
    }

    private static Stl ReadBinary(Stream stream)
    {
        throw new NotImplementedException();
    }

    private static Stl ReadAscii(Stream stream)
    {
        throw new NotImplementedException();
    }
}