
namespace Finnimon.Math.Test.PolyLineShapeTests;

public class PolyLineShapeIndexingTest
{
    [TestCase(0, 0, 0, 1, 1, 1)]
    public void IndexedGet(float x0, float y0, float z0, float x1, float y1, float z1)
    {
        Vertex3D first = new(x0, y0, z0);
        Vertex3D second = new(x1, y1, z1);
        PolyLineShape3D shape = new([first, second]);
        var lineSegment = shape[0];
        Assert.That(lineSegment.Start, Is.EqualTo(first));
        Assert.That(lineSegment.End, Is.EqualTo(second));
        var lineSegment1 = shape[1];
        Assert.That(lineSegment1.Start, Is.EqualTo(second));
        Assert.That(lineSegment1.End, Is.EqualTo(first));
    }
}
