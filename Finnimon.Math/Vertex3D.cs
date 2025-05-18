using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Math;

namespace Finnimon.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public readonly struct Vertex3D(double x = 0, double y = 0, double z = 0) : IEquatable<Vertex3D>
{
    public readonly double X = x, Y = y, Z = z;

    #region properties

    public double Length => Distance(0, 0, 0);
    public double SquaredLength => SquaredDistance(0, 0, 0);

    #endregion

    #region span access

    public double this[int index] => index >= 0 && index < 3
        ? Unsafe.Add(ref Unsafe.AsRef(in X), index)
        : throw new IndexOutOfRangeException();

    public int Count => 3;

    public static implicit operator ReadOnlySpan<double>(Vertex3D vertex) =>
        MemoryMarshal.CreateReadOnlySpan(ref Unsafe.AsRef(in vertex.X), 3);

    #endregion

    #region operators

    public static Vertex3D operator +(Vertex3D vertex, Vertex3D other) => vertex.Add(other);
    public static Vertex3D operator -(Vertex3D vertex, Vertex3D other) => vertex.Subtract(other);
    public static Vertex3D operator *(Vertex3D vertex, double scalar) => vertex.Scale(scalar);
    public static Vertex3D operator *(double scalar, Vertex3D vertex) => vertex.Scale(scalar);
    public static Vertex3D operator /(Vertex3D vertex, double divisor) => vertex.Divide(1 / divisor);


    public static double operator *(Vertex3D vertex, Vertex3D other) => vertex.Dot(other);
    public static Vertex3D operator ^(Vertex3D vertex, Vertex3D other) => vertex.Cross(other);

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public static bool operator ==(Vertex3D vertex, Vertex3D other) =>
        vertex.X == other.X && vertex.Y == other.Y && vertex.Z == other.Z;

    public static bool operator !=(Vertex3D vertex, Vertex3D other) => !(vertex == other);

    #endregion

    #region functions

    #region floating calc

    public double Dot(Vertex3D other) => Dot(other.X, other.Y, other.Z);
    public double Dot(double x, double y, double z) => X * x + Y * y + Z * z;

    public double Distance(Vertex3D other) => Sqrt(SquaredDistance(other));
    public double Distance(double x, double y, double z) => Sqrt(SquaredDistance(x, y, z));

    public double SquaredDistance(double x, double y, double z)
    {
        var xDiff = x - X;
        var yDiff = y - Y;
        var zDiff = z - Z;
        return xDiff * xDiff + yDiff * yDiff + zDiff * zDiff;
    }

    public double SquaredDistance(Vertex3D other) => SquaredDistance(other.X, other.Y, other.Z);

    #endregion

    #region pseudo transforms

    public Vertex3D Cross(Vertex3D other) => Cross(other.X, other.Y, other.Z);

    public Vertex3D Cross(double x, double y, double z)
        => new(
            x: Y * z - Z * y,
            y: Z * x - X * z,
            z: X * y - Y * x
        );

    public Vertex3D Scale(double factor) => new Vertex3D(factor * X, factor * Y, factor * Z);
    private Vertex3D Divide(double divisor) => new(X / divisor, Y / divisor, Z / divisor);
    public Vertex3D Normalize() => this / Length;


    public Vertex3D Add(double x, double y, double z) => new Vertex3D(X + x, Y + y, Z + z);
    public Vertex3D Add(Vertex3D other) => Add(other.X, other.Y, other.Z);

    public Vertex3D Subtract(double x, double y, double z) => new(X - x, Y - y, Z - z);
    public Vertex3D Subtract(Vertex3D other) => Subtract(other.X, other.Y, other.Z);

    #endregion

    #region angles

    public double AngleTo(Vertex3D other)
    {
        var dot = this * other;
        var magnitudes = Length * other.Length;
        if (magnitudes == 0)
            throw new InvalidOperationException("Cannot compute angle with zero-length vector.");
        return Acos(Clamp(dot / magnitudes, -1.0, 1.0));
    }

    public double SignedAngleTo(Vertex3D other, Vertex3D axis)
    {
        axis = axis.Normalize();
        var v1 = Normalize();
        var v2 = other.Normalize();
        var cross = v1 ^ v2;
        var dot = v1 * v2;
        var angle = Acos(Clamp(dot, -1.0, 1.0));
        double sign = Sign(axis * cross);
        return angle * sign;
    }

    #endregion

    #endregion

    #region static

    public static readonly Vertex3D XAxis = new(x: 1);
    public static readonly Vertex3D YAxis = new(y: 1);
    public static readonly Vertex3D ZAxis = new(z: 1);

    #endregion

    #region equality

    public bool Equals(Vertex3D other) => this == other;

    public override bool Equals(object? obj) => obj is Vertex3D other && this == other;

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    #endregion
}