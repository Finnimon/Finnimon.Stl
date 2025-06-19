using System.Formats.Asn1;

namespace Finnimon.Math;

public sealed record Mesh3D(Triangle3D[] Triangles) : IVolume3D
{
    #region lazy properties
    public Vertex3D Centroid => VertexCentroid;
    private Vertex3D? _vertexCentroid = null;
    public Vertex3D VertexCentroid => (_vertexCentroid ??= CalculateCentroid(Triangles, MeshCentroidType.Vertex));
    private Vertex3D? _areaCentroid = null;
    public Vertex3D AreaCentroid => (_areaCentroid ??= CalculateCentroid(Triangles, MeshCentroidType.Area));
    private Vertex3D? _volumeCentroid = null;
    public Vertex3D VolumeCentroid => (_volumeCentroid ??= CalculateCentroid(Triangles, MeshCentroidType.Volume));
    private float _area = float.NaN;
    public float Area => (_area = _area is not float.NaN ? _area : CalculateSurfaceArea(Triangles));
    private float _volume = float.NaN;
    public float Volume => (_volume = _volume is not float.NaN ? _volume : CalculateVolume(Triangles));
    #endregion


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
            .Select((float Area, Vertex3D Centroid) (tri) =>
            {
                var area = tri.Area;
                return (area, tri.Centroid * area);
            })
            .Aggregate(
                (float Area, Vertex3D Centroid) () => (0, Vertex3D.Zero),
                (accum, source) => (accum.Area + source.Area, accum.Centroid + source.Centroid),
                (subtotal1, subtotal2) => (subtotal1.Area + subtotal2.Area, subtotal1.Centroid + subtotal2.Centroid),
                total => total.Centroid / total.Area
            );

    private static Vertex3D CalculateVolumeCentroid(IEnumerable<Triangle3D> triangles)
        => triangles
            .AsParallel()
            .Select((Vertex3D Centroid, float Volume) (tri) => ((tri.A + tri.B + tri.C), SignedSixTetrahedronVolume(tri)))
            .Select((float Volume, Vertex3D Centroid) (x) => (x.Volume, x.Centroid * x.Volume))
            .Aggregate(
                (float Volume, Vertex3D Centroid) () => (0, Vertex3D.Zero),
                (aggregate, position) => (aggregate.Volume + position.Volume, aggregate.Centroid + position.Centroid),
                (subtotal1, subtotal2) => (subtotal1.Volume + subtotal2.Volume, subtotal1.Centroid + subtotal2.Centroid),
                total => total.Centroid / total.Volume * 0.25f
            );

    public static float CalculateVolumeParallel(IEnumerable<Triangle3D> triangles)
        => float.Abs(triangles.AsParallel().Sum(x => SignedSixTetrahedronVolume(in x))) / 6;

    #endregion

    #region sequential

    public static float CalculateVolumeSequential(Triangle3D[] triangles)
    {
        var volume = 0f;
        for (var i = 0; i < triangles.Length; i++) volume += SignedSixTetrahedronVolume(in triangles[i]);
        return float.Abs(volume / 6);
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
            var triangle = triangles[i];
            var curVolume = SignedSixTetrahedronVolume(in triangle);
            volume += curVolume;
            centroid += (triangle.A + triangle.B + triangle.C) * curVolume;
        }
        return centroid / volume * 0.25f;
    }

    private static Vertex3D CalculateAreaCentroidSeq(Triangle3D[] triangles)
    {
        var centroid = Vertex3D.Zero;
        var area = 0f;
        for (var i = 0; i < triangles.Length; i++)
        {
            var triangle = triangles[i];
            var curArea = triangle.Area;
            area += curArea;
            centroid += triangle.Centroid * curArea;
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


    private static float SignedSixTetrahedronVolume(in Triangle3D baseTriangle)
    => baseTriangle.A * (baseTriangle.B ^ baseTriangle.C);


    #endregion

    #region pseudo constants

    public static Mesh3D Empty { get; } = EmptyMesh();
    private static Mesh3D EmptyMesh()
    {
        Mesh3D mesh = new([]);
        mesh._area = 0;
        mesh._volume = 0;
        mesh._vertexCentroid = Vertex3D.Zero;
        mesh._areaCentroid = Vertex3D.Zero;
        mesh._volumeCentroid = Vertex3D.Zero;
        return mesh;
    }
    #endregion
}
