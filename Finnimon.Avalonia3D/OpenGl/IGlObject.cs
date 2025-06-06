namespace Finnimon.Avalonia3D.OpenGl;

public interface IGlObject
{
    public int Id { get; }

    public void Bind();
    public void Unbind();
    public void Delete();
}