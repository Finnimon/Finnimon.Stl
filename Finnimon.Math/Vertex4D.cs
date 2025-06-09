using System.Runtime.InteropServices;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
public readonly record struct Vertex4D(float X, float Y, float Z, float W)
{
    public Vertex4D(in Vertex3D xyz,float w):this(xyz.X, xyz.Y, xyz.Z, w){}
}