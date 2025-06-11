using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly record struct Vertex4D(float X, float Y, float Z, float W)
{
    public Vertex4D(in Vertex3D xyz,float w):this(xyz.X, xyz.Y, xyz.Z, w){}
    
    public Vertex3D XYZ=> new (X, Y, Z);
    public static Vertex4D operator *(in Vertex4D v, float scalar)
        =>new(v.X*scalar,v.Y*scalar,v.Z*scalar,v.W*scalar);
    public static Vertex4D operator *(float scalar,in Vertex4D v)
        =>new(v.X*scalar,v.Y*scalar,v.Z*scalar,v.W*scalar);
    public static Vertex4D operator /(in Vertex4D v, float divisor)
        =>v*(1.0f/divisor);
}