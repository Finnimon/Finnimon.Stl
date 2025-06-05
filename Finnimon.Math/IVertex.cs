namespace Finnimon.Math;

public interface IVertex
{
    public float this[int index] { get; }
    public int Count { get; }
}