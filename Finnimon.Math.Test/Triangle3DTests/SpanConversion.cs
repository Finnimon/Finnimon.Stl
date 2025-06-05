using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Finnimon.Math.Test.Triangle3DTests;

public class SpanConversion
{
    [TestCase(1,2,3,4,5,6,7,8,9)]
    public void TriangleTest(float ax, float ay, float az, float bx, float by,float bz, float cx, float cy, float cz)
    {
        var a = new Vertex3D(ax, ay, az);
        var b = new Vertex3D(bx, by, bz);
        var c = new Vertex3D(cx, cy, cz);
        var triangle = new Triangle3D(a, b, c);
        ReadOnlySpan<float> triangleSpan = triangle;
        foreach (var VARIABLE in triangleSpan)
        {
            Console.WriteLine(VARIABLE);
        }
    }
    
    [Test]
    public void TriangleArrayTest()
    {
        var a = new Vertex3D(0, 1, 2);
        var b = new Vertex3D(3, 4, 5);
        var c = new Vertex3D(6, 7, 8);
        var triangle = new Triangle3D(a, b, c);
        Triangle3D[] triangleArray = [triangle];
        var span = MemoryMarshal.CreateReadOnlySpan(
            ref Unsafe.As<Triangle3D, float>(ref Unsafe.AsRef(in triangleArray.AsSpan()[0])),
            length: 9
        );

        for (int i = 0; i < 9; i++)
        {
            Assert.AreEqual(span[i],i);
        }
        
    }
    
}