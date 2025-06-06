using System.Diagnostics;
using System.Reflection;
using Avalonia;
using Avalonia.Input;
using Finnimon.Avalonia3D.OpenGl;
using Finnimon.Math;
using OpenTK;
using OpenTK.Mathematics;
using OpenTKAvalonia;

namespace Finnimon.Avalonia3D;

public class MeshView : BaseTkOpenGlControl
{
    private Mesh3D Mesh { get; set; }
    RenderPipeline<Triangle3D> RenderPipeline { get; set; }
    public OrbitCamera Camera { get; }=new(float.Pi/4,new(),1,0,0);
    private ShaderProgram _shader;
    private bool _initialized = false;
    private bool _isDragging = false;
    private Point _lastPos;

    public MeshView() : this(null)
    {
    }

    public MeshView(Mesh3D mesh)
    {
        Mesh = mesh;
    }

    public void SetMesh(Mesh3D mesh)
    {
        if (mesh is null) return;
        Mesh = mesh;
        Camera.LookAt(mesh.Centroid);
        if (_initialized) RenderPipeline= new RenderPipeline<Triangle3D>(mesh.Triangles);
    }

    public void RemoveMesh()
    {
        Mesh = null;
        if (_initialized) RenderPipeline= new RenderPipeline<Triangle3D>([]);
    }

    protected override void OpenTkInit()
    {
        GL.ClearColor(0.2f, 0.3f, 0.3f, 1.0f);
        _shader = ShaderProgram.FromFiles("Shaders/default.vert", "Shaders/default.frag");
        RenderPipeline = new RenderPipeline<Triangle3D>(Mesh?.Triangles??[]);
        _initialized = true;
        
    }

    private float Red = 0f;
    private int direction = 1;
    protected override void OpenTkRender()
    {
        GL.Enable(EnableCap.DepthTest);

        GL.ClearColor(Red, 0f, 0.2f, 1.0f);
        Red += direction*0.002f;
        if (Red > 1.0f)
        {
            direction = -1;
            Red = 1.0f;
        }
        else if (Red < 0.0f)
        {
            direction = 1;
            Red = 0.0f;
        }
        //Clear the previous frame
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //Update camera pos etc
        DoUpdate();

        //Render the object(s)
        DoRender();

        //Clean up the opengl state back to how we got it
        GL.Disable(EnableCap.DepthTest);
    }

    protected override void OpenTkTeardown()
    {
        Console.WriteLine("Teardown.");
        RenderPipeline = null;
        _shader?.Unbind();
        _shader = null;
    }
    private void DoRender()
    {
        _shader.Bind();
        var (model, view, projection) = Camera.CreateRenderMatrices((float)(Bounds.Width / Bounds.Height));
        _shader.SetMatrix4(nameof(model), ref model);
        _shader.SetMatrix4(nameof(view), ref view);
        _shader.SetMatrix4(nameof(projection), ref projection);
        RenderPipeline?.Render();
    }

    private void DoUpdate()
    {
        if(Mesh is null) return;
        if(RenderPipeline is not null) return;
        RenderPipeline=new(Mesh.Triangles);
    }
    
    #region Camera updates

    
    
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isDragging = true;
        e.Pointer.Capture(this);
        _lastPos = e.GetPosition(null);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        if (!_isDragging)
            return;

        //Work out the change in position
        var pos = e.GetPosition(null);

        var deltaX = pos.X - _lastPos.X;
        var deltaY = pos.Y - _lastPos.Y;
        _lastPos = pos;

        const float sensitivity = 0.005f;

        //Yaw is a function of the change in X
        Camera.MoveToSides(-(float)deltaX * sensitivity);
        
        Camera.MoveUp((float)deltaY * sensitivity);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var scrollDelta = e.Delta.Y; //negative is out, positive is in
        Camera.MoveForwards((float)scrollDelta); 
    }

    #endregion
}
