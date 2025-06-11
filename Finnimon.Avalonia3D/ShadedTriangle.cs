using System.Runtime.InteropServices;
using Finnimon.Math;

namespace Finnimon.Avalonia3D;

[StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16 * 3)]
public readonly record struct ShadedTriangle(
    Vertex4D A,
    Vertex4D B,
    Vertex4D C
)
{
    public static ShadedTriangle FromTriangle(in Triangle3D triangle,in Vertex3D shadeAgainst )
    {
        var shade = triangle.Normal * shadeAgainst / 2f + 0.5f;
        shade = shade * 0.8f + 0.2f;
        return new(
            new Vertex4D(triangle.A,shade),
            new Vertex4D(triangle.B,shade),
            new Vertex4D(triangle.C,shade)
            );
    }

    public static ShadedTriangle[] ShadedTriangles(Triangle3D[] triangles, in Vertex3D? shadeAgainst=null)
    {
        var length = triangles.Length;
        var shaded = new ShadedTriangle[length];
        var shader=shadeAgainst?.Normalize()?? new Vertex3D(1f,4f,15f).Normalize();
        for (var i = 0; i < length; i++) shaded[i]= FromTriangle(triangles[i],in shader);
        return shaded;
    }
    
}