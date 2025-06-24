using Finnimon.Stl;
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
}

public interface ILayerFillAlgorithm
{
    public Vertex3D[] Fill(ISlicingLayer slicingLayer);
}
public sealed record PolyLineShape2D(Vertex2D[] LineStrip)
{
    public LineSegment2D this[int index]
    {
        if(index.OutsideInclusiveRange(0, Count-1)) throw new IndexOutOfRangeException();
    var secondIndex = index + 1;
        if(secondIndex>=Count) secondIndex=0;
        return new LineSegment2D(LineStrip[index], LineStrip[secondIndex]);
    }
    public int Count => LineStrip.Length;
    public float Circumference
    {
        var circ=0f;
        var previous=LineStrip[0];
for (int i = 1; i < LineStrip.Length; i++)
{
    var current = LineStrip[i];
    circ += previous.Distance(current);
}
    }
}
public readonly struct LineSegment2D(Vertex2D Start, Vertex2D End)
{
    public float Length => Start.Distance(End);
    public Vertex2D Direction => End - Start;
    public Vertex2D Traverse(float distance) => Start + Direction.Normalize() * distance;
    public bool Intersects(LineSegment2D other)
    {
        var direction = this.Direction;
        var otherDirection = other.Direction;
        if (direction.IsParallel(otherDirection)) return false;

    }
    public bool IsOnSegment(Vertex2D vertex) => throw new NotImplementedException();
}
