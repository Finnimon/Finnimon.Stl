using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Explicit, Size = 36)]
public readonly record struct Triangle3D(
    [field: FieldOffset(00)] Vertex3D A,
    [field: FieldOffset(12)] Vertex3D B,
    [field: FieldOffset(24)] Vertex3D C) : IFace3D
{
    public Vertex3D Centroid => (A + B + C) / 3;
    public Vertex3D Normal => AbAcCross(A, B, C).Normalize();
    public float Area => AbAcCross(A, B, C).Length / 2;
    public float Circumference => (A - B).Length + (B - C).Length + (C - A).Length;
    private static Vertex3D AbAcCross(Vertex3D a, Vertex3D b, Vertex3D c) => (b - a) ^ (c - a);
    
    public static implicit operator ReadOnlySpan<float>(in Triangle3D triangle)
        => MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<Triangle3D, float>(ref Unsafe.AsRef(in triangle)),
            length: 9
        );

    public unsafe double this[int i]
    {
        get
        {
            if(i.InsideInclusiveRange(0,8))
                fixed (Triangle3D* ptr = &this)
                    return ((float*)ptr)[i];
            throw new IndexOutOfRangeException();
        }
    }
}