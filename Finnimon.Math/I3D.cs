namespace Finnimon.Math;

public interface I3D : IVertex
{
    float X { get; }
    float Y { get; }
    float Z { get; }

    public new float this[int index] => index switch
    {
        0 => X,
        1 => Y,
        2 => Z,
        _ => throw new IndexOutOfRangeException()
    };
}