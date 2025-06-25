using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public readonly record struct Line2D(
    [field: FieldOffset(0)] Vertex2D Start,
    [field: FieldOffset(8)] Vertex2D End)
: ILine<Vertex2D>
{
    public float Length => Start.Distance(End);
    public Vertex2D Direction => End - Start;
    public Vertex2D NormalDirection => Direction.Normalize();

    public static Line2D FromDirection(in Vertex2D start, in Vertex2D direction)
    => new(start, start + direction);

    public Vertex2D Traverse(float distance) => Start + NormalDirection * distance;

    public Vertex2D TraverseOnSegment(float distance)
    {
        var length = Length;
        distance /= length;
        distance = float.Clamp(distance, 0f, 1f);
        return distance * Direction + Start;
    }

    public static unsafe explicit operator Line2D(in Vertex2D start)
    {
        fixed (Vertex2D* startPtr = &start) return ((Line2D*)startPtr)[0];
    }
}
