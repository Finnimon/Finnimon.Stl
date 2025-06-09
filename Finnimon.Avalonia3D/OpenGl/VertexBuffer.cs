namespace Finnimon.Avalonia3D.OpenGl;

public class VertexBuffer<T>() :IBufferObject<T> where T : struct
{
    public int Id { get; }=GL.GenBuffer();
    public void Bind() => this.BindBuffer();

    public void Unbind() => this.UnbindBuffer();
    public BufferTarget BufferTarget => BufferTarget.ArrayBuffer;
    
}