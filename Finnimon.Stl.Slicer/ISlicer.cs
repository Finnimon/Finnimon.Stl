using Finnimon.Math;
public interface ISlicer
{
    public ToolPathRegion[] Slice(Mesh3D mesh);
}

public sealed record ToolPathRegion(ToolPathPosition[] Positions, RegionInfo Info);
public readonly record struct ToolPathPosition(Vertex3D TcpPosition, Vertex3D ToolDirection);
public sealed record RegionInfo(ToolMode Mode, string? Description = "");
public enum ToolMode
{
    Travel,
    Cut
}

public interface ISlicingLayer
{
    public PolyLineShape3D TrueShape();
    public PolyLineShape2D Flatten();
    public Vertex3D[] Inflate(Vertex2D[] flat);
    public Vertex2D Deflate(Vertex3D[] spatial);
}

public interface ILayerFillAlgorithm
{
    public Vertex3D[] Fill(ISlicingLayer slicingLayer);
}


public interface ILayerCutter
{
    ISlicingLayer[] Cut(Mesh3D mesh);
}

public sealed record PlanarLayer(
    Vertex3D Normal,
    PolyLineShape3D Bounds
)
{
}
