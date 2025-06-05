using System.Diagnostics;
using Finnimon.Math;

namespace Finnimon.Stl;

public static class MeshCalculator
{
    public static IEnumerable<IFace3D> SortedByArea(this IEnumerable<IFace3D> mesh)
        => mesh.Select(face => (face, face.Area))
            .OrderBy(x => x.Area)
            .Select(x => x.face);

    public static float SurfaceArea<T>(this IEnumerable<T> mesh)
        where T : IFace3D
        => mesh.Sum(face => face.Area);

    public static double Volume(this IEnumerable<Triangle3D> mesh) => mesh.Sum(SignedTetrahedronVolume).Abs();

    public static Vertex3D Centroid<T>(this IEnumerable<T> mesh)
        where T : IFace3D
    {
        var meshCollection = mesh as ICollection<T> ?? mesh.ToList();
        var count = meshCollection.Count;
        var centroid = meshCollection.Aggregate(new Vertex3D(), (centroid, face) => face.Centroid + centroid);
        return centroid / count;
    }

    #region private helpers


    private static double SignedTetrahedronVolume(Triangle3D baseTriangle)
    {
        var (a, b, c) = baseTriangle;
        return (a * (b ^ c)) / 6.0;
    }

    #endregion
}