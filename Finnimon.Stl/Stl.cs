using Finnimon.Math;

namespace Finnimon.Stl;

public sealed record Stl(string Header, IReadOnlyList<Triangle3D> Triangles):IBody3D
{
    private Vertex3D? _centroid;
    private double? _surfaceArea;
    private double? _volume;
    public Vertex3D Centroid => _centroid??=Triangles.Centroid();
    public double SurfaceArea => _surfaceArea??=Triangles.SurfaceArea();
    public double Volume => _volume??=Triangles.Volume();
}