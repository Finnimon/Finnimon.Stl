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
        => Mesh3D.CalculateCentroid(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Vertex);
    [Benchmark]
    public Vertex3D ParallelAreaCentroid()
        => Mesh3D.CalculateCentroid(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Area);
    [Benchmark]
    public Vertex3D ParallelVolumeCentroid()
        => Mesh3D.CalculateCentroid(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Volume);
    
    
    [Benchmark]
    public Vertex3D SequentialVertexCentroid()
        => Mesh3D.CalculateCentroidSeq(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Vertex);
    [Benchmark]
    public Vertex3D SequentialAreaCentroid()
        => Mesh3D.CalculateCentroidSeq(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Area);
    [Benchmark]
    public Vertex3D SequentialVolumeCentroid()
        => Mesh3D.CalculateCentroidSeq(_mesh?.Triangles??throw new IncompleteInitialization(), MeshCentroidType.Volume);
    #endregion
    #region volume
    [Benchmark]
    public double ParallelVolume()=>Mesh3D.CalculateVolume(_mesh?.Triangles??throw new IncompleteInitialization());
    [Benchmark]
    public double SequentialVolume()=>Mesh3D.CalculateVolumeSeq(_mesh?.Triangles??throw new IncompleteInitialization());
    #endregion
    #region surface
    [Benchmark]
    public double ParallelSurface()=>Mesh3D.CalculateSurfaceArea(_mesh?.Triangles??throw new IncompleteInitialization());
    [Benchmark]
    public double SequentialSurface()=>Mesh3D.CalculateSurfaceAreaSeq(_mesh?.Triangles??throw new IncompleteInitialization());
    #endregion
}