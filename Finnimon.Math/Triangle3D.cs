namespace Finnimon.Math;

public sealed record Triangle3D(Vertex3D A, Vertex3D B, Vertex3D C):IFace3D
{
    public Vertex3D Centroid=>(A+B+C)/3;
    public double Area => Normal.Length/2;
    public double Circumference =>(A-B).Length+(B-C).Length+(C-A).Length;
    public Vertex3D Normal=>(B-A)^(C-A);
}