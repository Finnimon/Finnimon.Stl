using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Math;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Explicit, Pack = 4, Size = 12)]
public readonly record struct Vertex3D(
    [field: FieldOffset(0)] float X = 0,
    [field: FieldOffset(4)] float Y = 0,
    [field: FieldOffset(8)] float Z = 0) : I3D
{
    #region properties

    public int Count => 3;
    public float Length => float.Sqrt(SquaredLength);
    public float SquaredLength => X * X + Y * Y + Z * Z;
    public static Vertex3D Zero => new();

    #endregion

    #region operators

    public static Vertex3D operator +(in Vertex3D vertex, in Vertex3D other) =>
        new(vertex.X + other.X, vertex.Y + other.Y, vertex.Z + other.Z);

    public static Vertex3D operator -(in Vertex3D vertex, in Vertex3D other) =>
        new(vertex.X - other.X, vertex.Y - other.Y, vertex.Z - other.Z);

    public static Vertex3D operator *(in Vertex3D vertex, float scalar) =>
        new(vertex.X * scalar, vertex.Y * scalar, vertex.Z * scalar);

    public static Vertex3D operator *(float scalar, in Vertex3D vertex) =>
        new(vertex.X * scalar, vertex.Y * scalar, vertex.Z * scalar);

    public static Vertex3D operator /(in Vertex3D vertex, float divisor) =>
        new(vertex.X / divisor, vertex.Y / divisor, vertex.Z / divisor);


    public static float operator *(in Vertex3D vertex, in Vertex3D other) =>
        vertex.X * other.X + vertex.Y * other.Y + vertex.Z * other.Z;

    public static Vertex3D operator ^(in Vertex3D vertex, in Vertex3D other) => vertex.Cross(other);

    #endregion

    #region functions

    #region floating calc

    public float Dot(in Vertex3D other) => X * other.X + Y * other.Y + Z * other.Z;
    public float Dot(float x, float y, float z) => X * x + Y * y + Z * z;

    public float Distance(in Vertex3D other) => float.Sqrt(SquaredDistance(other.X, other.Y, other.Z));
    public float Distance(float x = 0, float y = 0, float z = 0) => float.Sqrt(SquaredDistance(x, y, z));

    public float SquaredDistance(float x = 0, float y = 0, float z = 0)
    {
        var xDiff = x - X;
        var yDiff = y - Y;
        var zDiff = z - Z;
        return xDiff * xDiff + yDiff * yDiff + zDiff * zDiff;
    }

    public float SquaredDistance(in Vertex3D other)
    {
        var xDiff = other.X - X;
        var yDiff = other.Y - Y;
        var zDiff = other.Y - Z;
        return xDiff * xDiff + yDiff * yDiff + zDiff * zDiff;
    }

    #endregion

    #region pseudo transforms

    public Vertex3D Cross(in Vertex3D other)
        => new(
            X: Y * other.Z - Z * other.Y,
            Y: Z * other.X - X * other.Z,
            Z: X * other.Y - Y * other.X
        );

    public Vertex3D Cross(float x, float y, float z)
        => new(
            X: Y * z - Z * y,
            Y: Z * x - X * z,
            Z: X * y - Y * x
        );

    public Vertex3D Scale(float factor) => new Vertex3D(factor * X, factor * Y, factor * Z);
    private Vertex3D Divide(float divisor) => new(X / divisor, Y / divisor, Z / divisor);
    public Vertex3D Normalize() => this / Length;


    public Vertex3D Add(float x = 0, float y = 0, float z = 0) => new Vertex3D(X + x, Y + y, Z + z);
    public Vertex3D Add(in Vertex3D other) => new(X + other.X, Y + other.Y, Z + other.Z);

    public Vertex3D Subtract(float x = 0, float y = 0, float z = 0) => new(X - x, Y - y, Z - z);
    public Vertex3D Subtract(in Vertex3D other) => new(X - other.X, Y - other.Y, Z - other.Z);

    #endregion

    #region angles

    public float AngleTo(in Vertex3D other)
    {
        var dot = this * other;
        var magnitudes = Length * other.Length;
        if (magnitudes == 0)
            throw new InvalidOperationException("Cannot compute angle with zero-length vector.");
        return float.Acos(float.Clamp(dot / magnitudes, -1, 1));
    }

    public float SignedAngleTo(in Vertex3D other, in Vertex3D normalAxis)
    {
        var v1 = Normalize();
        var v2 = other.Normalize();
        var cross = v1 ^ v2;
        var dot = v1 * v2;
        var angle = float.Acos(float.Clamp(dot, -1f, 1f));
        var sign = Sign(normalAxis * cross);
        return angle * sign;
    }

    #endregion

    #endregion

    #region static

    public static Vertex3D XAxis => new(X: 1);
    public static Vertex3D YAxis => new(Y: 1);
    public static Vertex3D ZAxis => new(Z: 1);

    #endregion

    #region equality

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public bool Equals(Vertex3D? other) => other is { } vert && X == vert.X && Y == vert.Y && Z == vert.Z;

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(in Vertex3D vertex, ReadOnlySpan<float> span) => vertex == (Vertex3D)span;
    public static bool operator !=(in Vertex3D vertex, ReadOnlySpan<float> span) => vertex != (Vertex3D)span;

    #endregion

    #region conversions

    #region spans

    private static readonly IndexOutOfRangeException IndexerException = new("Index was not 0<=index<3");

    public unsafe float this[int index]
    {
        get
        {
            if (index.OutsideInclusiveRange(0, 2)) throw IndexerException;
            fixed (Vertex3D* p = &this) return ((float*)p)[index];
        }
    }

    public static unsafe implicit operator ReadOnlySpan<float>(in Vertex3D vertex)
    {
        fixed (Vertex3D* p = &vertex) return new ReadOnlySpan<float>(p, 3);
    }

    public static implicit operator Vertex3D(Span<float> span) => Unsafe.As<float, Vertex3D>(ref span[0]);

    public static implicit operator Vertex3D(ReadOnlySpan<float> span) => new(span[0], span[1], span[2]);

    #endregion

    #region System.numerics

    public static implicit operator Vector3(in Vertex3D vertex) => new(vertex.X, vertex.Y, vertex.Z);
    public static implicit operator Vertex3D(in Vector3 vertex) => new(vertex.X, vertex.Y, vertex.Z);

    #endregion

    #endregion
}