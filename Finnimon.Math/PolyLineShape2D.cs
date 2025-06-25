namespace Finnimon.Math;

public sealed record PolyLineShape2D(Vertex2D[] LineStrip)
{
    public Line2D this[int index]
    {
        get
        {
            if (index.InsideInclusiveRange(0, Count - 2))
                return (Line2D)LineStrip[index];
                if (index == Count - 1) return new Line2D(LineStrip[index], LineStrip[0]);
            throw new IndexOutOfRangeException();
        }
    }
    public int Count => LineStrip.Length;
    private float _circumference = float.NaN;
    public float Circumference => (_circumference = (_circumference is float.NaN) ? CalculateCircumference() : _circumference);

    private float CalculateCircumference()
    {
        var circ = 0f;
        var previous = LineStrip[0];
        for (int i = 1; i < LineStrip.Length; i++)
        {
            var current = LineStrip[i];
            circ += previous.Distance(current);
            previous = current;
        }
        return circ;
    }
}
