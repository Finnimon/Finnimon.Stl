namespace Finnimon.Math;

public interface IBody3D : IComplex3D
{
    public double SurfaceArea { get; }
    public double Volume { get; }
}