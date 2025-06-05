namespace Finnimon.Math;

public sealed record CoordinateSystem(Vertex3D X, Vertex3D Y, Vertex3D Z, Vertex3D Position)
{
    public static readonly CoordinateSystem Identity = new(
        X: Vertex3D.XAxis,
        Y: Vertex3D.YAxis,
        Z: Vertex3D.ZAxis,
        Position: new Vertex3D()
    );

    public Vertex3D Transform(Vertex3D vertex)
    {
        var relative = vertex - Position;

        return new Vertex3D(
            X: relative * X,
            Y: relative * Y,
            Z: relative * Z
        );
    }
}