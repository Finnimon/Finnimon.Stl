using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SysVec2=System.Numerics.Vector2;
using static System.Math;
namespace Finnimon.Math;
[StructLayout(LayoutKind.Explicit, Pack=4, Size=8)]
public readonly record struct Vertex2D([field: FieldOffset(0)] float X=0, [field: FieldOffset(4)] float Y=0) : IVertex
{
    #region properties
    public float Length => (float) Sqrt(SquaredLength);
    public float SquaredLength => X * X + Y * Y;
    public int Count => 2;
    #endregion

    #region operators
    public static Vertex2D operator +(Vertex2D vertex, Vertex2D other) => new Vertex2D(vertex.X+other.X, vertex.Y+other.Y);
    public static Vertex2D operator -(Vertex2D vertex, Vertex2D other) => new Vertex2D(vertex.X-other.X, vertex.Y-other.Y);
    public static Vertex2D operator *(Vertex2D vertex, float scalar) => new Vertex2D(vertex.X*scalar, vertex.Y*scalar);
    public static Vertex2D operator *(float scalar, Vertex2D vertex) => vertex*scalar;
    public static Vertex2D operator /(Vertex2D vertex, float divisor) => new Vertex2D(vertex.X/divisor,vertex.Y/divisor);
    public static float operator /(float divident, Vertex2D divisor) => divident / (float)divisor;
    public static float operator *(Vertex2D vertex, Vertex2D other) => vertex.X*other.X + vertex.Y*other.Y;
    #endregion
    
    #region conversion

    #region spans
    private static readonly IndexOutOfRangeException IndexerException = new("Index was not 0<=index<3");
    public unsafe float this[int index]
    {
        get
        {
            if (index.OutsideInclusiveRange(0, 1)) throw IndexerException;
            fixed(Vertex2D* p = &this) return ((float*)p)[index];
        }
    }

    public static unsafe implicit operator ReadOnlySpan<float>(in Vertex2D vertex)
    {
        fixed (Vertex2D* p = &vertex) return new ReadOnlySpan<float>(p, 3);
    }

    public static implicit operator Vertex2D(Span<float> span) => new(span[0], span[1]);
    public static implicit operator Vertex2D(ReadOnlySpan<float> span) => new(span[0], span[1]);
    #endregion
    
    public static implicit operator float(Vertex2D v) => v.Length;
    public static implicit operator Vertex3D(Vertex2D v)=>new Vertex3D(X:v.X, Y:v.Y);
    public static implicit operator Vertex2D(Vertex3D v)=>new Vertex2D(v.X, v.Y);
    public static implicit operator SysVec2(Vertex2D v)=>new SysVec2((float)v.X, (float)v.Y);
    public static implicit operator Vertex2D(SysVec2 v)=>new Vertex2D(v.X, v.Y);
    #endregion
}