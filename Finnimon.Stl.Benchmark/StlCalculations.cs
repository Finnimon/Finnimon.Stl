using BenchmarkDotNet.Attributes;
using Finnimon.Math;
using Microsoft.VisualBasic.CompilerServices;

namespace Finnimon.Stl.Benchmark;

public class StlCalculations
{
    private Mesh3D? _mesh;

    [GlobalSetup]
    public void GlobalSetup()
    {
        var stl = StlReader.Read(File.OpenRead("Files/big.stl"));
        _mesh = new(stl.Facets.Select(x => x.Triangle).ToArray());
    }

    #region centroid
    [Benchmark]
    public Vertex3D ParallelVertexCentroid()
        => Mesh3D.CalculateCentroidParallel(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Vertex);
    [Benchmark]
    public Vertex3D ParallelAreaCentroid()
        => Mesh3D.CalculateCentroidParallel(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Area);
    [Benchmark]
    public Vertex3D ParallelVolumeCentroid()
        => Mesh3D.CalculateCentroidParallel(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Volume);
    
    
    [Benchmark]
    public Vertex3D SequentialVertexCentroid()
        => Mesh3D.CalculateCentroidSequential(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Vertex);
    [Benchmark]
    public Vertex3D SequentialAreaCentroid()
        => Mesh3D.CalculateCentroidSequential(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Area);
    [Benchmark]
    public Vertex3D SequentialVolumeCentroid()
        => Mesh3D.CalculateCentroidSequential(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Volume);
    #endregion
    #region volume
    [Benchmark]
    public double ParallelVolume()=>Mesh3D.CalculateVolumeParallel(_mesh?.Triangles??throw new IncompleteInitialization());
    [Benchmark]
    public double SequentialVolume()=>Mesh3D.CalculateVolumeSequential(_mesh?.Triangles??throw new IncompleteInitialization());
    #endregion
    #region surface
    [Benchmark]
    public double ParallelSurface()=>Mesh3D.CalculateSurfaceAreaParallel(_mesh?.Triangles??throw new IncompleteInitialization());
    [Benchmark]
    public double SequentialSurface()=>Mesh3D.CalculateSurfaceAreaSequential(_mesh?.Triangles??throw new IncompleteInitialization());
    #endregion
}