namespace Finnimon.Math.Test.Vertex3DTests;

public class SpanConversion
{
    [TestCase(0,0,0)]
    [TestCase(1,2,3)]
    [TestCase(float.MaxValue,float.Epsilon,float.MinValue)]
    public void FromToConversionReadOnlySpan(float x, float y, float z)
    {
        var vertex = new Vertex3D(x, y, z);
        ReadOnlySpan<float> span = vertex;
        Vertex3D vertex2 = span;
        Assert.That(vertex2, Is.EqualTo(vertex));
    }
    [TestCase(0,0,0)]
    [TestCase(1,2,3)]
    [TestCase(float.MaxValue,float.Epsilon,float.MinValue)]
    public void FromToConversionSpan(float x, float y, float z)
    {
        var vertex = new Vertex3D(x, y, z);
        float[] array = [x, y, z];
        Span<float> span= array;
        Vertex3D vertex2 = span;
        Assert.That(vertex2, Is.EqualTo(vertex));
    }

    [TestCase(0, 0, 0)]
    [TestCase(1, 2, 3)]
    [TestCase(float.MaxValue, float.Epsilon, float.MinValue)]
    public void IndexedAccess(float x, float y, float z)
    {
        var vertex = new Vertex3D(x, y, z);
        Assert.Multiple(() =>
        {
            Assert.That(vertex[0], Is.EqualTo(vertex.X));
            Assert.That(vertex[1], Is.EqualTo(vertex.Y));
            Assert.That(vertex[2], Is.EqualTo(vertex.Z));
        });
    }
}