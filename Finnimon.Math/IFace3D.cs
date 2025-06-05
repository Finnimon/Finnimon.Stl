namespace Finnimon.Math;

public interface IFace3D : IComplex3D, IFace
{
    public Vertex3D Normal { get; }
}