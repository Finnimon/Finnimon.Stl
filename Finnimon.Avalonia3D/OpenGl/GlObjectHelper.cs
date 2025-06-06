using System.Runtime.CompilerServices;

namespace Finnimon.Avalonia3D.OpenGl;

public static class GlObjectHelper
{
    public static void BindBuffer<T>(this IBufferObject<T> me) where T : struct => GL.BindBuffer( me.BufferTarget, me.Id);
    public static void UnbindBuffer<T>(this IBufferObject<T> me) where T : struct =>GL.BindBuffer(me.BufferTarget, 0);
    public static void CopyData<T>(this IBufferObject<T> me,T[] data) where T : struct
    {
        GL.BufferData(me.BufferTarget, data.Length*me.ItemByteSize, data, me.BufferUsage);
    }
    public static void DeleteBuffer<T>(this IBufferObject<T> me) where T : struct => GL.DeleteBuffer(me.Id);
    public static void DeleteArray(this IGlObject me) => GL.DeleteVertexArray(me.Id);
    public static void BindArray(this IGlObject me) => GL.BindVertexArray(me.Id);
    public static void UnbindArray(this IGlObject me) => GL.BindVertexArray(0);

    public static TBuffer CreateAndCopyInto<TBuffer, TData>(TData[] data) 
        where TData : struct 
        where TBuffer : IBufferObject<TData>, new()
    {
        TBuffer buffer = new ();
        buffer.Bind();
        buffer.CopyData(data);
        return buffer;
    }       

    public static uint ByteSize<T>(ICollection<T> data) where T : struct
    =>(uint)(Unsafe.SizeOf<T>()*data.Count);
    
    public static uint FloatCount<T>(ICollection<T> data) where T: struct
    => ByteSize(data)/sizeof(float);

    public static uint ByteSize<T>() where T: struct
        => (uint)Unsafe.SizeOf<T>();
}