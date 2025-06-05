namespace Finnimon.Avalonia3D.OpenGl;

public class VertexArray():IGlObject
{
    public int Id { get; } = GL.GenVertexArray();
    private int _linked = 0;
    public void Bind()=>this.BindArray();
    public void Unbind()=>this.UnbindArray();
    public void Link<T>(int size,  VertexBuffer<T> vbo) where T : struct
    {
        Bind();
        vbo.Bind();
        GL.VertexAttribPointer(_linked, size, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(_linked);
        _linked++;
        Unbind();
    }
}