using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Finnimon.Math;
using OpenTK.Mathematics;

namespace Finnimon.Avalonia3D;

public static class MathExt
{
    public static Vector3 ToOpenTk(this Vertex3D vertex)=>new(vertex.X, vertex.Y, vertex.Z);

    public static Vertex3D ToFinnimon(this Vector3 vector)=>new(vector.X,vector.Y,vector.Z);
    public static ReadOnlySpan<float> Vertices(this Mesh3D mesh)
    {
        var triangles = mesh.Triangles;
        var ptr= Unsafe.As<Triangle3D, float>(ref triangles.AsSpan()[0]);
        return MemoryMarshal.CreateSpan(ref ptr, triangles.Length*9);
    }
    
}