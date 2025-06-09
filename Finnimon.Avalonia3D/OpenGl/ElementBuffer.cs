namespace Finnimon.Avalonia3D.OpenGl;

public class ElementBuffer():IBufferObject<uint>
{
    public int Id { get; }=GL.GenBuffer();
    public BufferTarget BufferTarget => BufferTarget.ElementArrayBuffer;

    

    public void Bind() => this.BindBuffer();
    public void Unbind()=>this.UnbindBuffer();
}

