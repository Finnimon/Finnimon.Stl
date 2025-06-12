using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;

namespace Finnimon.Avalonia3D;

public class BaseOpenTkView : OpenGlControlBase
{
    
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        throw new NotImplementedException();
    }
}