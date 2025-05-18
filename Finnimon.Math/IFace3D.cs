namespace Finnimon.Math;

public interface IFace3D : IComplex3D
{
    public double Area { get; }
    public double Circumference { get; }
    public Vertex3D Normal { get; }
}