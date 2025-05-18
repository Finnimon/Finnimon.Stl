using System.Diagnostics;
using Finnimon.Math;

namespace Finnimon.Stl;

public static class MeshCalculator
{
    public static IEnumerable<IFace3D> SortedByArea(this IEnumerable<IFace3D> mesh)
        => mesh.Select(face => (face, face.Area))
            .OrderBy(x => x.Area)
            .Select(x => x.face);

    public static double SurfaceArea(this IEnumerable<IFace3D> mesh) => mesh.Sum(face => face.Area);
    public static double Volume(this IEnumerable<Triangle3D> mesh) => mesh.Sum(SignedTetrahedronVolume).Abs();

    public static Vertex3D Centroid(this IEnumerable<IFace3D> mesh)
    {
        var meshCollection=mesh as ICollection<IFace3D>??mesh.ToList();
        var count=meshCollection.Count;
        var centroid = meshCollection.Coalesce((face, centroid) => face.Centroid + centroid, new Vertex3D());
        return centroid/count;
    }

    #region private helpers

    private static TResult Coalesce<TIn,TResult>(this IEnumerable<TIn> me, Func<TIn, TResult, TResult> reducer, TResult start)
    {
        var result = start;
        foreach (var item in me)
            result=reducer(item,result);

        return result;
    }

    private static double SignedTetrahedronVolume(Triangle3D baseTriangle)
    {
        var (a, b, c) = baseTriangle;
        return (a * (b ^ c)) / 6.0;
    }

    #endregion
}