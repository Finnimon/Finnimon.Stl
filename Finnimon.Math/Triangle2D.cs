using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Explicit)]
public readonly record struct Triangle2D(
    [field: FieldOffset(8*0)] Vertex2D A,
    [field: FieldOffset(8*1)] Vertex2D B,
    [field: FieldOffset(8*2)] Vertex2D C) 
    : IFace
{
    public float Area
    {
        get
        {
            var ab = B - A;
            var ac = C - A;
            var normalLength = ab.X * ac.Y - ab.Y * ab.X;
            return System.Math.Abs(normalLength) / 2;
        }
    }

    public float Circumference => (A - B).Length + (B - C).Length + (C - A).Length;
}