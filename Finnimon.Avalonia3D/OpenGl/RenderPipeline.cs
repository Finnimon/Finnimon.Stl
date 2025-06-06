using Finnimon.Math;

namespace Finnimon.Avalonia3D.OpenGl;

public sealed class RenderPipeline<T> where T : struct
{
    private VertexArray VertexArray { get; }
    private VertexBuffer<T> VertexBuffer { get; }
    // private ElementBuffer IndicesBuffer { get; }
    public T[] Vertices { get; }

    public RenderPipeline(T[] vertices)
    {
        Vertices = vertices;
        VertexArray = new();
        VertexArray.Bind();
        VertexBuffer = GlObjectHelper.CreateAndCopyInto<VertexBuffer<T>, T>(Vertices);
        VertexBuffer.Bind();
        VertexArray.Link((int)GlObjectHelper.ByteSize<T>()/sizeof(float),VertexBuffer);
        // IndicesBuffer=GlObjectHelper.CreateAndCopyInto<ElementBuffer,uint>(indices);
    }

    public void Render()
    {
        VertexArray.Bind();
        // VertexBuffer.Bind();
        // IndicesBuffer.Bind();
        // GL.DrawElements(PrimitiveType.Triangles, Indices.Length, DrawElementsType.UnsignedInt, 0);
        VertexBuffer.Bind();
        GL.DrawArrays(PrimitiveType.Triangles,0,(int) GlObjectHelper.FloatCount(Vertices));
        // GL.DrawArrays(PrimitiveType.Triangles,0,(int) (Vertices.Count*GlObjectHelper.ByteSize(Vertices)/sizeof(float)));
        VertexBuffer.Unbind();
        VertexArray.Unbind();
        // VertexBuffer.Unbind();
        // IndicesBuffer.Unbind();
    }
    
    private static uint[] AutoIndices(IList<T> vertices)
    {
        var floatCount = GlObjectHelper.ByteSize(vertices) / sizeof(float);
        var indices = new uint[floatCount];
        for (uint i = 0; i < floatCount; i++) indices[i] = i;
        return indices;
    }

    ~RenderPipeline()
    {
        VertexArray.Unbind();
        VertexArray.Delete();
        VertexBuffer.Unbind();
        ((IBufferObject<T>)VertexBuffer).Delete();
    }
    
}