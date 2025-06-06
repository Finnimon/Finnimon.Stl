using System.Formats.Asn1;

namespace Finnimon.Math;

public sealed record Mesh3D(Triangle3D[] Triangles, Vertex3D Centroid, float Volume,float Area) : IVolume3D
{
    public Mesh3D(Triangle3D[] triangles, MeshCentroidType centroidType = MeshCentroidType.Vertex) : this(triangles,
        CalculateCentroid(triangles, centroidType), CalculateVolume(triangles),CalculateSurfaceArea(triangles))
    {
    }

    #region Calculations
    public static float CalculateSurfaceArea(Triangle3D[] triangles)
        => SystemInfo.ProcessorCount > 4
            ? CalculateSurfaceAreaParallel(triangles)
            : CalculateSurfaceAreaSequential(triangles);

    public static float CalculateVolume(Triangle3D[] triangles)
        => SystemInfo.ProcessorCount > 4
            ? CalculateVolumeParallel(triangles)
            : CalculateVolumeSequential(triangles);

    public static Vertex3D CalculateCentroid(Triangle3D[] triangles, MeshCentroidType type)
        => SystemInfo.ProcessorCount > 4
            ? CalculateCentroidParallel(triangles, type)
            : CalculateCentroidSequential(triangles, type);

    #region parallel

    public static float CalculateSurfaceAreaParallel(IEnumerable<Triangle3D> triangles) =>
        triangles.AsParallel().Sum(x => x.Area);

    public static Vertex3D CalculateCentroidParallel(Triangle3D[] triangles, MeshCentroidType type)
        => type switch
        {
            MeshCentroidType.Vertex => CalculateVertexCentroid(triangles),
            MeshCentroidType.Area => CalculateAreaCentroid(triangles),
            MeshCentroidType.Volume => CalculateVolumeCentroid(triangles),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };


    private static Vertex3D CalculateVertexCentroid(Triangle3D[] triangles)
        => triangles
            .AsParallel()
            .Select(x => x.A + x.B + x.C)
            .Aggregate(
                () => Vertex3D.Zero,
                (aggregate, centroid) => aggregate + centroid,
                (subtotal1, subtotal2) => subtotal1 + subtotal2,
                total => total / (triangles.Length * 3)
            );

    private static Vertex3D CalculateAreaCentroid(IEnumerable<Triangle3D> triangles)
        => triangles
            .AsParallel()
            .Select(x => (x.Area, x.Centroid))
            .Select((float Area, Vertex3D Centroid) (x) => (x.Area, x.Centroid * x.Area))
            .Aggregate(
                (float Area, Vertex3D Centroid) () => (0, Vertex3D.Zero),
                (accum, source) => (accum.Area + source.Area, accum.Centroid + source.Centroid),
                (subtotal1, subtotal2) => (subtotal1.Area + subtotal2.Area, subtotal1.Centroid + subtotal2.Centroid),
                total => total.Centroid / total.Area
            );

    private static Vertex3D CalculateVolumeCentroid(IEnumerable<Triangle3D> triangles)
        => triangles
            .AsParallel()
            .Select((Vertex3D Centroid, float Volume) (x) => ((x.A + x.B + x.C) / 4, SignedTetrahedronVolume(x)))
            .Select((float Volume, Vertex3D Centroid) (x) => (x.Volume, x.Centroid * x.Volume))
            .Aggregate(
                (float Volume, Vertex3D Centroid) () => (0, Vertex3D.Zero),
                (aggregate, position) => (aggregate.Volume + position.Volume, aggregate.Centroid + position.Centroid),
                (subtotal1, subtotal2) =>
                    (subtotal1.Volume + subtotal2.Volume, subtotal1.Centroid + subtotal2.Centroid),
                total => total.Centroid / total.Volume
            );

    public static float CalculateVolumeParallel(IEnumerable<Triangle3D> triangles)
        => float.Abs(triangles.AsParallel().Sum(x => SignedTetrahedronVolume(in x)));

    #endregion

    #region sequential

    public static float CalculateVolumeSequential(Triangle3D[] triangles)
    {
        var volume = 0f;
        for (var i = 0; i < triangles.Length; i++) volume += SignedTetrahedronVolume(in triangles[i]);
        return float.Abs(volume);
    }

    public static float CalculateSurfaceAreaSequential(Triangle3D[] triangles)
    {
        var surface = 0f;
        for (var i = 0; i < triangles.Length; i++) surface += triangles[i].Area;
        return surface;
    }


    public static Vertex3D CalculateCentroidSequential(Triangle3D[] triangles, MeshCentroidType type)
        => type switch
        {
            MeshCentroidType.Vertex => CalculateVertexCentroidSeq(triangles),
            MeshCentroidType.Area => CalculateAreaCentroidSeq(triangles),
            MeshCentroidType.Volume => CalculateVolumeCentroidSeq(triangles),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };

    private static Vertex3D CalculateVolumeCentroidSeq(Triangle3D[] triangles)
    {
        var centroid = Vertex3D.Zero;
        var volume = 0f;
        for (var i = 0; i < triangles.Length; i++)
        {
            var curVolume = SignedTetrahedronVolume(triangles[i]);
            var curCentroid = (triangles[i].A + triangles[i].B + triangles[i].C) / 4;

            volume += curVolume;
            centroid += curCentroid * curVolume;
        }

        return centroid / volume;
    }

    private static Vertex3D CalculateAreaCentroidSeq(Triangle3D[] triangles)
    {
        var centroid = Vertex3D.Zero;
        var area = 0f;
        for (var i = 0; i < triangles.Length; i++)
        {
            var curArea = triangles[i].Area;
            var curCentroid = triangles[i].Centroid;
            area += curArea;
            centroid += curCentroid * curArea;
        }

        return centroid / area;
    }

    private static Vertex3D CalculateVertexCentroidSeq(Triangle3D[] triangles)
    {
        var centroid = Vertex3D.Zero;
        for (var i = 0; i < triangles.Length; i++)
            centroid += triangles[i].A + triangles[i].B + triangles[i].C;
        return centroid / (3 * triangles.Length);
    }

    #endregion

    private static float SignedTetrahedronVolume(Triangle3D baseTriangle)
    {
        const float sixth = 1f / 6f;
        var (a, b, c) = baseTriangle;
        return sixth * (a * (b ^ c));
    }

    private static float SignedTetrahedronVolume(in Triangle3D baseTriangle)
    {
        const float sixth = 1f / 6f;
        var (a, b, c) = baseTriangle;
        return sixth * (a * (b ^ c));
    }

    #endregion
}