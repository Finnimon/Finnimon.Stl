namespace Finnimon.Math;

public interface ILine<TVertex>
{
    public TVertex Start { get; }
    public TVertex End { get; }
    public TVertex Direction { get; }
    public TVertex NormalDirection { get; }
    public float Length { get; }
    public TVertex Traverse(float distance);
    public TVertex TraverseOnSegment(float distance);
}
