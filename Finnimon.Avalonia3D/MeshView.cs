using Avalonia;
using Avalonia.Input;
using Finnimon.Avalonia3D.OpenGl;
using Finnimon.Math;
using OpenTK.Mathematics;
using OpenTKAvalonia;

namespace Finnimon.Avalonia3D;

public class MeshView : BaseTkOpenGlControl
{
    #region props and fields
    public OrbitCamera Camera { get; }=new(float.Pi/4,Vertex3D.Zero,1,0,0);
    public RenderMode RenderModeFlags { get; set; } = RenderMode.Solid|RenderMode.WireFrame;
    public Vertex4D BgColor { get; set; } = new (0.2f, 0, 0.2f, 1);
    public Vertex4D WireFrameColor
    {
        get => _wireFrameColor;
        set
        {
            _newWireFrameColor = true;
            _wireFrameColor = value;
        }
    }
    
    private ShadedTriangle[] Triangles { get; set; } = [];
    private ShaderProgram SolidShader { get; set; }
    private ShaderProgram WireFrameShader { get; set; }
    
    private int _vbo;
    private int _vao;
    private bool _newMesh;

    private Vertex4D _wireFrameColor = new (1, 1, 1, 1);
    private bool _newWireFrameColor = true;
    
    private bool _isDragging = false;
    private Point _lastPos;
    
    #endregion
    
    public MeshView() : this(null)
    {
    }

    public MeshView(Mesh3D mesh)
    {
        SetMesh(mesh);
    }
    
    public void ClearMesh()=>SetMesh(null);
    
    public void SetMesh(Mesh3D mesh)
    {
        Camera.LookAt(mesh?.Centroid??Vertex3D.Zero);
        _newMesh = true;
        Triangles = ShadedTriangle.ShadedTriangles(mesh?.Triangles??[]);
    }


    protected override void OpenTkInit()
    {
        GL.ClearColor(BgColor.X,BgColor.Y, BgColor.Z, BgColor.W);
        SolidShader = ShaderProgram.FromFiles("Shaders/default");
        WireFrameShader = ShaderProgram.FromFiles("Shaders/solidcolor");
        _vao=GL.GenVertexArray();
    }

    protected override void OpenTkRender()
    {
        DoUpdate();
        DoRender();
    }

    protected override void OpenTkTeardown()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vbo);
        GL.BindVertexArray(0);
        GL.DeleteVertexArray(_vao);
        SolidShader?.Unbind();
        SolidShader = null;
        WireFrameShader?.Unbind();
        WireFrameShader = null;
    }

    #region do render
    
    private void DoRender()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.Enable(EnableCap.CullFace);
        GL.BindVertexArray(_vao);
        GL.ClearColor(BgColor.X,BgColor.Y, BgColor.Z, BgColor.W);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        
        var (model, view, projection) = Camera.CreateRenderMatrices((float)(Bounds.Width / Bounds.Height));

        //actual Render Calls
        
        if((RenderModeFlags & RenderMode.Solid)==RenderMode.Solid) SolidRender(ref model, ref view, ref projection);
        if((RenderModeFlags&RenderMode.WireFrame)==RenderMode.WireFrame) WireFrameRender(ref model, ref view, ref projection);
        
        var err = GL.GetError();
        if (err != ErrorCode.NoError) Console.WriteLine($"GL Error: {err}");
        
        
        //Clean up the opengl state back to how we got it
        GL.Disable(EnableCap.DepthTest);
        GL.Disable(EnableCap.CullFace);
        GL.BindVertexArray(0);
    }

    private void SolidRender(ref Matrix4 model, ref Matrix4 view, ref Matrix4 projection)
    {
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
        SolidShader.Bind();
        SolidShader.SetMatrix4(nameof(model), ref model);
        SolidShader.SetMatrix4(nameof(view), ref view);
        SolidShader.SetMatrix4(nameof(projection), ref projection);
        GL.DrawArrays(PrimitiveType.Triangles, 0,Triangles.Length*3);
        SolidShader.Unbind();
    }

    private void WireFrameRender(ref Matrix4 model, ref Matrix4 view, ref Matrix4 projection)
    {
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Line);
        WireFrameShader.Bind();
        WireFrameShader.SetMatrix4(nameof(model), ref model);
        WireFrameShader.SetMatrix4(nameof(view), ref view);
        WireFrameShader.SetMatrix4(nameof(projection), ref projection);
        GL.LineWidth(2.5f);
        GL.DrawArrays(PrimitiveType.Triangles, 0,Triangles.Length*3);
        GL.LineWidth(1f);
        WireFrameShader.Unbind();
        GL.PolygonMode(MaterialFace.FrontAndBack,PolygonMode.Fill);
    }

    private static readonly int MaxDrawSize = Environment.SystemPageSize * 1024;
    private static void SplitTriangleDrawCall(PrimitiveType primitiveType, int triangleCount, int vertexByteSize)
    {
        var triangleByteSize = vertexByteSize * 3;
        var vertexCount=triangleCount*3;
        
        var totalSize=vertexCount*vertexByteSize;
        var maxDrawByteSize = MaxDrawSize;
        var singleCall = totalSize < maxDrawByteSize;
        
        if (singleCall)
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0,vertexCount);
            return;
        }
        
        var trianglesPerDrawCall=maxDrawByteSize/triangleByteSize;
        var verticesPerDrawCall = trianglesPerDrawCall * 3;
        var drawCallByteSize=trianglesPerDrawCall*triangleByteSize;
        var chunkedCallCount=totalSize/drawCallByteSize;
        var drawnVertices = 0;
        for (var i = 0; i < chunkedCallCount; i++)
        {
            GL.DrawArrays(PrimitiveType.Triangles,drawnVertices,verticesPerDrawCall);
            drawnVertices+=verticesPerDrawCall;
        }
        var remaining=vertexCount-drawnVertices;
        if (remaining<=0) return;
        GL.DrawArrays(PrimitiveType.Triangles,drawnVertices,remaining);
    }

    #endregion
    
    #region do update

    
    private void DoUpdate()
    {
        UpdateWfShader();
        UpdateMesh();
    }

    private void UpdateWfShader()
    {
        if (!_newWireFrameColor) return;
        _newWireFrameColor = false;
        WireFrameShader.Bind();
        WireFrameShader.SetVec4("uniform_color",WireFrameColor.ToOpenTk());
        WireFrameShader.Unbind();
    }


    private void UpdateMesh()
    {
        if (!_newMesh) return;
        _newMesh = false;
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
        if(_vbo!=0) GL.DeleteBuffer(_vbo);
        _vbo = GL.GenBuffer();
                
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer,(int) GlObjectHelper.ByteSize(Triangles), Triangles, BufferUsageHint.StaticDraw);
        
        const int attribLocation = 0;
        if (SolidShader.GetAttribLocation("shaded_vertex_position") != attribLocation
            || WireFrameShader.GetAttribLocation("solid_color_vertex_position") != attribLocation)
        {
            Console.WriteLine("Shaders misaligned");
            throw new Exception("Shaders misaligned");
        }
        GL.VertexAttribPointer(attribLocation,4, VertexAttribPointerType.Float, false, 0, 0);
        GL.EnableVertexAttribArray(attribLocation);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
        GL.BindVertexArray(0);
    }

    #endregion

    #region Camera updates
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        _isDragging = true;
        e.Pointer.Capture(this);
        _lastPos = e.GetPosition(null);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) => _isDragging = false;

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