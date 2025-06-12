using System.Runtime.InteropServices;
using Finnimon.Math;

namespace Finnimon.Avalonia3D;

[StructLayout(LayoutKind.Explicit, Size = 12 * 6)]
public readonly record struct ShadedTriangle(
    [field: FieldOffset(0)] Vertex3D A,
    [field: FieldOffset(12)] Vertex3D NormalA,
    [field: FieldOffset(24)] Vertex3D B,
    [field: FieldOffset(36)] Vertex3D NormalB,
    [field: FieldOffset(48)] Vertex3D C,
    [field: FieldOffset(60)] Vertex3D NormalC
)
{
    public static ShadedTriangle FromTriangle(in Triangle3D triangle)
    {
        var normal = triangle.Normal;
        return new(
            triangle.A,normal,triangle.B,normal,triangle.C,normal
            );
    }

    public static ShadedTriangle[] ShadedTriangles(Triangle3D[] triangles)
    {
        var length = triangles.Length;
        var shaded = new ShadedTriangle[length];
        for (var i = 0; i < length; i++) shaded[i]= FromTriangle(triangles[i]);
        return shaded;
    }
    
}