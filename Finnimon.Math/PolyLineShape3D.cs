namespace Finnimon.Math;

public sealed record PolyLineShape3D(Vertex3D[] LineStrip)
{

    public Line3D this[int index]
    {
        get
        {
            if (index.InsideInclusiveRange(0, Count - 2))
                return (Line3D)LineStrip[index];
            if (index == Count - 1) return new Line3D(LineStrip[index], LineStrip[0]);
            throw new IndexOutOfRangeException();
        }
    }
    public int Count => LineStrip.Length;
    private float _circumference = float.NaN;
    public float Circumference => (_circumference = (_circumference is float.NaN) ? CalculateCircumference() : _circumference);
    private Vertex3D _vertexCentroid=Vertex3D.NaN;
    public Vertex3D VertexCentroid=>(_vertexCentroid=_vertexCentroid!=Vertex3D.NaN?_vertexCentroid:CaclulateVertexCentroid());
    private Vertex3D CaclulateVertexCentroid()
    {
        Vertex3D centroid=Vertex3D.Zero;
        for(var i=0;i<LineStrip.Length;i++) centroid+=LineStrip[i];
        return centroid/LineStrip.Length;
    }


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
