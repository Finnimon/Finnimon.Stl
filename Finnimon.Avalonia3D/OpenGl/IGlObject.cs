namespace Finnimon.Avalonia3D.OpenGl;

public interface IGlObject :IDisposable
{
    public int Id { get; }
    void IDisposable.Dispose() => Delete();

    public void Bind();
    public void Unbind();
    public void Delete()=>this.DeleteArray();
}