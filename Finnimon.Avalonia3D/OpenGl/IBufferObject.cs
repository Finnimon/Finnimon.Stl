using System.Runtime.CompilerServices;

namespace Finnimon.Avalonia3D.OpenGl;

public interface IBufferObject<T> : IGlObject where T : struct
{
    public BufferTarget BufferTarget { get; }
    public int ItemByteSize=>Unsafe.SizeOf<T>();
    public BufferUsageHint BufferUsage =>BufferUsageHint.StaticDraw;
    void IGlObject.Delete()=>this.DeleteBuffer();
}