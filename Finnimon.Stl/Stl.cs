using Finnimon.Math;

namespace Finnimon.Stl;

public sealed record Stl(string? Name, string Header, IReadOnlyList<StlFacet> Facets)
{
    public Mesh3D ToMesh()
    {
        var triangles =new Triangle3D[Facets.Count];
        for (var i = 0; i < Facets.Count; i++) triangles[i] = Facets[i].Triangle;
        return new Mesh3D(triangles);
    }
}