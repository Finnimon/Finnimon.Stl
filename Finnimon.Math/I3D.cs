namespace Finnimon.Math;

public interface I3D
{
    double X { get; }
    double Y { get; }
    double Z { get; }
    public Vertex3D AsVertex3D()=>new Vertex3D(X, Y, Z);
}