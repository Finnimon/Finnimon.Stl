using System.Diagnostics;
using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using Finnimon.Avalonia3D.OpenGl;
using Finnimon.Math;
using OpenTK.Mathematics;
using OpenTKAvalonia;

namespace Finnimon.Avalonia3D;

public class MeshView : BaseTkOpenGlControl
{
    #region props and fields

    public OrbitCamera Camera { get; }
    public RenderMode RenderModeFlags { get; set; }

    public Mesh3D Mesh
    {
        get => _mesh;
        set
        {
            _mesh = value ?? Mesh3D.Empty;
            _triangleBuffer = ShadedTriangle.ShadedTriangles(Mesh.Triangles);
            _newMesh = true;
            Camera.LookAt(Mesh.Centroid);
            if (Mesh.Triangles.Length <= 0) return;
            Camera.Distance=Mesh.Triangles[0].A.Distance(Mesh.Centroid)*2;
        }
    }

    public Color AvaloniaBackgroundColor
    {
        get => new (
            (byte)(byte.MaxValue * BackgroundColor.A),
            (byte)(byte.MaxValue * BackgroundColor.R),
            (byte)(byte.MaxValue * BackgroundColor.G),
            (byte)(byte.MaxValue * BackgroundColor.B));
        set => BackgroundColor=new(value.R,value.G,value.B,value.A);
    }
    public Color4 BackgroundColor { get; set; }

    public Color AvaloniaSolidColor
    {
        get => new (
            (byte)(byte.MaxValue * SolidColor.A),
            (byte)(byte.MaxValue * SolidColor.R),
            (byte)(byte.MaxValue * SolidColor.G),
            (byte)(byte.MaxValue * SolidColor.B));
        set => SolidColor=new(value.R,value.G,value.B,value.A);
    }
    public Color4 SolidColor
    {
        get => _solidColor;
        set
        {
            _newSolidColor = true;
            _solidColor = value;
        }
    }
    public Color AvaloniaWireframeColor
    {
        get => new (
            (byte)(byte.MaxValue * WireframeColor.A),
            (byte)(byte.MaxValue * WireframeColor.R),
            (byte)(byte.MaxValue * WireframeColor.G),
            (byte)(byte.MaxValue * WireframeColor.B));
        set => WireframeColor=new(value.R,value.G,value.B,value.A);
    }
    public Color4 WireframeColor
    {
        get => _wireFrameColor;
        set
        {
            _newWireframeColor = true;
            _wireFrameColor = value;
        }
    }

    public double Fps { get; private set; }
    private readonly Stopwatch _frameTimer;
    private ShadedTriangle[] Triangles { get; set; }
    private ShaderProgram SolidShader { get; set; }
    private ShaderProgram WireframeShader { get; set; }
    private Mesh3D _mesh;
    private int _vbo;
    private int _vao;
    private bool _newMesh;

    private Color4 _wireFrameColor;
    private bool _newWireframeColor;
    private Color4 _solidColor;
    private bool _newSolidColor;

    private bool _isDragging;
    private Point _lastPos;
    private ShadedTriangle[] _triangleBuffer;

    #endregion

    public MeshView() : this(Mesh3D.Empty)
    {
    }

    public MeshView(Mesh3D mesh)
    {
        Camera = new OrbitCamera(float.Pi / 4, Vertex3D.Zero, 1, 0, 0);
        Triangles = [];
        RenderModeFlags = RenderMode.Solid | RenderMode.Wireframe;
        _frameTimer = new Stopwatch();
        SolidColor = Color4.CornflowerBlue;
        WireframeColor = Color4.White;
        BackgroundColor = Color4.DarkViolet;
        Mesh = mesh;
    }

    public void ClearMesh() => Mesh = Mesh3D.Empty;


    protected override void OpenTkInit()
    {
        _frameTimer.Restart();
        GL.ClearColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
        SolidShader = ShaderProgram.FromFiles("./Shaders/default");
        WireframeShader = ShaderProgram.FromFiles("./Shaders/solidcolor");
        _vao = GL.GenVertexArray();
        LogGlError();
    }

    protected override void OpenTkRender()
    {
        DoUpdate();
        DoRender();
        DoCsysRender();
    }


    protected override void OpenTkTeardown()
    {
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.DeleteBuffer(_vbo);
        GL.BindVertexArray(0);
        GL.DeleteVertexArray(_vao);
        SolidShader?.Unbind();
        SolidShader = null;
        WireframeShader?.Unbind();
        WireframeShader = null;
    }

    #region do render
    
    private void DoCsysRender()
    {
    }
    private void DoRender()
    {
        GL.Enable(EnableCap.DepthTest);
        GL.BindVertexArray(_vao);
        GL.ClearColor(BackgroundColor.R, BackgroundColor.G, BackgroundColor.B, BackgroundColor.A);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        var (model, view, projection) = Camera.CreateRenderMatrices((float)(Bounds.Width / Bounds.Height));

        //actual Render Calls

        if ((RenderModeFlags & RenderMode.Solid) == RenderMode.Solid) SolidRender(ref model, ref view, ref projection);
        if ((RenderModeFlags & RenderMode.Wireframe) == RenderMode.Wireframe) WireframeRender(ref model, ref view, ref projection);
        LogGlError();
        //Clean up the opengl state back to how we got it
        GL.Disable(EnableCap.DepthTest);
        GL.BindVertexArray(0);
    }

    private static void LogGlError()
    {
        var err = GL.GetError();
        if (err != ErrorCode.NoError)
        {
            Debug.WriteLine($"GL Error: {err}");
        }
        
    }


    private void SolidRender(ref Matrix4 model, ref Matrix4 view, ref Matrix4 projection)
    {
        GL.Enable(EnableCap.CullFace);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        SolidShader.Bind();
        SolidShader.SetMatrix4(nameof(model), ref model);
        SolidShader.SetMatrix4(nameof(view), ref view);
        SolidShader.SetMatrix4(nameof(projection), ref projection);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Triangles.Length * 3);

        SolidShader.Unbind();
        GL.Disable(EnableCap.CullFace);
    }

    private void WireframeRender(ref Matrix4 model, ref Matrix4 view, ref Matrix4 projection)
    {
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
        WireframeShader.Bind();
        WireframeShader.SetMatrix4(nameof(model), ref model);
        WireframeShader.SetMatrix4(nameof(view), ref view);
        WireframeShader.SetMatrix4(nameof(projection), ref projection);
        GL.Enable(EnableCap.PolygonOffsetLine);
        GL.PolygonOffset(-1f,-1f);
        GL.LineWidth(2f);
        GL.DepthMask(false);
        GL.DrawArrays(PrimitiveType.Triangles, 0, Triangles.Length * 3);
        GL.DepthMask(true);
        // SplitTriangleDrawCall((uint) Triangles.LongLength,(int)GlObjectHelper.ByteSize<ShadedTriangle>());
        GL.LineWidth(1f);
        GL.Disable(EnableCap.PolygonOffsetLine);
        WireframeShader.Unbind();
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
    }

    private static readonly int MaxDrawSize = Environment.SystemPageSize * 1024;

    [Obsolete]
    private static void SplitTriangleDrawCall(uint triangleCount, int vertexByteSize)
    {
        var triangleByteSize = vertexByteSize * 3;
        var vertexCount = triangleCount * 3;

        var totalSize = vertexCount * vertexByteSize;
        var maxDrawByteSize = MaxDrawSize;
        var singleCall = totalSize < maxDrawByteSize;

        if (singleCall)
        {
            GL.DrawArrays(PrimitiveType.Triangles, 0, (int)vertexCount);
            return;
        }

        var trianglesPerDrawCall = maxDrawByteSize / triangleByteSize;
        var verticesPerDrawCall = trianglesPerDrawCall * 3;
        var drawCallByteSize = trianglesPerDrawCall * triangleByteSize;
        var chunkedCallCount = totalSize / drawCallByteSize;
        var drawnVertices = 0;
        for (var i = 0; i < chunkedCallCount; i++)
        {
            GL.DrawArrays(PrimitiveType.Triangles, drawnVertices, verticesPerDrawCall);
            drawnVertices += verticesPerDrawCall;
        }

        var remaining = (int)(vertexCount - drawnVertices);
        if (remaining <= 0) return;
        GL.DrawArrays(PrimitiveType.Triangles, drawnVertices, remaining);
    }

    #endregion

    #region do update

    private void DoUpdate()
    {
        UpdateFps();
        UpdateShaders();
        UpdateMesh();
    }

    private void UpdateFps()
    {
        var elapsed = _frameTimer.ElapsedMilliseconds;
        _frameTimer.Restart();
        Fps = 1000.0 / elapsed;
    }

    private void UpdateShaders()
    { 
        UpdateSolidShaderLight();
        if (_newWireframeColor) UpdateShaderColor(WireframeShader, "uniform_color", in _wireFrameColor);
        if (_newSolidColor) UpdateShaderColor(SolidShader, "shaded_uniform_color", in _solidColor);
    }

    private void UpdateSolidShaderLight()
    {
        SolidShader.Bind();
        var light = Camera.UnitUp.Add(x: 0.1f, y: 0.2f,z:0.3f).Normalize();
        SolidShader.SetVec3("shade_against", light.ToOpenTk());
        SolidShader.Unbind();
    }

    private static void UpdateShaderColor(ShaderProgram shader, string name, in Color4 color)
    {
        shader.Bind();
        var vec4 = new Vector4(color.R, color.G, color.B, color.A);
        shader.SetVec4(name, vec4);
        shader.Unbind();
    }


    private void UpdateMesh()
    {
        if (!_newMesh) return;
        _newMesh = false;
        Triangles = _triangleBuffer;
        GL.BindVertexArray(_vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer,0);
        if (_vbo != 0) GL.DeleteBuffer(_vbo);
        _vbo = GL.GenBuffer();

        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer, (int)GlObjectHelper.ByteSize(Triangles), Triangles, BufferUsageHint.StaticDraw);

        var solidShaderLoc = SolidShader.GetAttribLocation("position");
        var wireFrameLoc = WireframeShader.GetAttribLocation("position");
        if (solidShaderLoc != wireFrameLoc)
        {
            Console.WriteLine($"Shaders misaligned Solid:{solidShaderLoc} Wireframe:{wireFrameLoc}");
            throw new Exception($"Shaders misaligned Solid:{solidShaderLoc} Wireframe:{wireFrameLoc}");
        }

        var positionLoc = solidShaderLoc;
        
        GL.VertexAttribPointer(positionLoc, 3, VertexAttribPointerType.Float, false, 6*sizeof(float), 0);
        GL.EnableVertexAttribArray(positionLoc);
        var normalLoc = SolidShader.GetAttribLocation("normal");
        GL.VertexAttribPointer(normalLoc,3,VertexAttribPointerType.Float,false, sizeof(float)*6,3*sizeof(float));
        GL.EnableVertexAttribArray(normalLoc);
        
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
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