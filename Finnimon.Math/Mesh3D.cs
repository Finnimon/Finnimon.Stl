namespace Finnimon.Math;

public sealed record Mesh3D(Triangle3D[] Triangles) : IVolume3D
{
    #region lazy properties
    public Vertex3D Centroid => VertexCentroid;
    private Vertex3D _vertexCentroid = Vertex3D.NaN;
    public Vertex3D VertexCentroid => (_vertexCentroid = _vertexCentroid != Vertex3D.NaN ? _vertexCentroid : CalculateCentroid(Triangles, MeshCentroidType.Vertex));
    private Vertex3D _areaCentroid = Vertex3D.NaN;
    public Vertex3D AreaCentroid => (_areaCentroid = _areaCentroid != Vertex3D.NaN ? _areaCentroid : CalculateCentroid(Triangles, MeshCentroidType.Area));
    private Vertex3D _volumeCentroid = Vertex3D.NaN;
    public Vertex3D VolumeCentroid => (_volumeCentroid = _volumeCentroid != Vertex3D.NaN ? _volumeCentroid : CalculateCentroid(Triangles, MeshCentroidType.Volume));
    private float _area = float.NaN;
    public float Area => (_area = _area is not float.NaN ? _area : CalculateSurfaceArea(Triangles));
    private float _volume = float.NaN;
    public float Volume => (_volume = _volume is not float.NaN ? _volume : CalculateVolume(Triangles));
    #endregion
    #region init fully

    public async Task InitializeLaziesAsync()
    => await Task.Run(InitializeLazies);

    private class InitAccum(
        Vertex3D vertexCentroid,
        Vertex3D areaCentroid,
        Vertex3D volumeCentroid,
        float volume,
        float area)
    {
        public Vertex3D VertexCentroid = vertexCentroid;
        public Vertex3D AreaCentroid = areaCentroid;
        public Vertex3D VolumeCentroid = volumeCentroid;
        public float Volume = volume;
        public float Area = area;

        public static InitAccum Combine(InitAccum subtotal1, InitAccum subtotal2)
        {
            subtotal1.VertexCentroid += subtotal2.VertexCentroid;
            subtotal1.AreaCentroid += subtotal2.AreaCentroid;
            subtotal1.VolumeCentroid += subtotal2.VolumeCentroid;
            subtotal1.Volume += subtotal2.Volume;
            subtotal1.Area += subtotal2.Area;
            return subtotal1;
        }

        public static InitAccum Total(InitAccum total, long triangleCount)
        => new(
            total.VertexCentroid / 3 / triangleCount,
            total.AreaCentroid / 3 / total.Area,
            total.VolumeCentroid / 4 / total.Volume,
            total.Volume / 6,
            total.Area
        );
        public void Deconstruct(
            out Vertex3D vertexCentroid,
            out Vertex3D areaCentroid,
            out Vertex3D volumeCentroid,
            out float volume,
            out float area
            )
        {
            vertexCentroid = VertexCentroid;
            areaCentroid = AreaCentroid;
            volumeCentroid = VolumeCentroid;
            volume = Volume;
            area = Area;
        }
    }

    public void InitializeLazies()
    {
        var triangles = Triangles;
        Vertex3D vertexCentroid = Vertex3D.Zero;
        Vertex3D areaCentroid = Vertex3D.Zero;
        Vertex3D volumeCentroid = Vertex3D.Zero;
        float volume = 0;
        float area = 0;
        for (long i = 0; i < triangles.LongLength; i++)
        {
            ref var tri = ref triangles[i];
            var (a, b, c) = tri;
            var cornerAvg = a + b + c;
            var curArea = tri.Area;
            var curVolume = SignedSixTetrahedronVolume(in tri);
            vertexCentroid += cornerAvg;
            areaCentroid += cornerAvg * curArea;
            volumeCentroid += cornerAvg * curVolume;
            volume += curVolume;
            area += curArea;

        }
        _vertexCentroid = vertexCentroid / 3 / triangles.LongLength;
        _areaCentroid = areaCentroid / 3 / area;
        _volumeCentroid = volumeCentroid / 4 / volume;
        _volume = volume / 6;
        _area = area;
    }

    public async Task InitializeLaziesParallel()
    {
        var coreCount = SystemInfo.ProcessorCount;
        var chunks = new (long start, long length)[coreCount];
        var chunkSize = Triangles.Length / coreCount;
        for (int core = 0; core < coreCount - 1; core++)
            chunks[core] = (core * chunkSize, chunkSize);
        var lastChunkOffset = chunkSize * (coreCount - 1);
        var lastChunkSize = Triangles.Length - lastChunkOffset;
        chunks[coreCount - 1] = (lastChunkOffset, lastChunkSize);
        var tasks = new Task<InitAccum>[coreCount];
        for (int core = 0; core < coreCount; core++)
        {
            var chunk = chunks[core];
            tasks[core] = Task.Run(() => HandleChunk(chunk.start, chunk.length, Triangles));
        }
        await Task.WhenAll(tasks);
        var result = tasks.Select(x => x.Result).Aggregate(InitAccum.Combine);
        (_vertexCentroid, _areaCentroid, _volumeCentroid, _volume, _area) = InitAccum.Total(result, Triangles.LongLength);
    }
    private static InitAccum HandleChunk(long start, long length, Triangle3D[] triangles)
    {
        Vertex3D vertexCentroid = Vertex3D.Zero;
        Vertex3D areaCentroid = Vertex3D.Zero;
        Vertex3D volumeCentroid = Vertex3D.Zero;
        float volume = 0;
        float area = 0;
        for (long i = start; i < start + length; i++)
        {
            ref var tri = ref triangles[i];
            var (a, b, c) = tri;
            var cornerAvg = a + b + c;
            var curArea = tri.Area;
            var curVolume = SignedSixTetrahedronVolume(in tri);
            vertexCentroid += cornerAvg;
            areaCentroid += cornerAvg * curArea;
            volumeCentroid += cornerAvg * curVolume;
            volume += curVolume;
            area += curArea;
        }
        return new(vertexCentroid, areaCentroid, volumeCentroid, volume, area);
    }


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
