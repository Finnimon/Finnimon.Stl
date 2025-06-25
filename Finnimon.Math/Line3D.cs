using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Explicit, Size = 24)]
public readonly record struct Line3D(
    [field: FieldOffset(00)] Vertex3D Start,
    [field: FieldOffset(12)] Vertex3D End)
: ILine<Vertex3D>
{
    public float Length => Start.Distance(End);
    public Vertex3D Direction => End - Start;
    public Vertex3D NormalDirection => Direction.Normalize();

    public static Line3D FromDirection(in Vertex3D start,in Vertex3D direction)
    =>new(start, start+direction);


    public Vertex3D Traverse(float distance) => Start + NormalDirection * distance;

    public Vertex3D TraverseOnSegment(float distance)
    {
        var length = Length;
        distance /= length;
        distance = float.Clamp(distance, 0f, 1f);
        return distance * Direction + Start;
    }
    public static unsafe explicit operator Line3D(in Vertex3D start)
    {
        fixed (Vertex3D* startPtr = &start) return ((Line3D*)startPtr)[0];
    }
}
