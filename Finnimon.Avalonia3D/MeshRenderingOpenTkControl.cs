using Avalonia.Input;
using Finnimon.Math;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTKAvalonia;
using Finnimon.Avalonia3D.Shaders;
using OpenTK.Graphics;
using Point = Avalonia.Point;

namespace Finnimon.Avalonia3D;

public class MeshRenderingOpenTkControl : BaseTkOpenGlControl
{
    #region OpenTk fields

    private int _vertexBufferObject;
    private int _vertexArrayObject;

    private Vector3 _cameraPosition = new(0, 2, 2);
    private Vector3 _cameraFront;
    private Vector3 _up = Vector3.UnitZ;
    private float _fov = 45;
    private float _pitch = -40;
    private float _yaw = 90f;
    private float _modelRotationDegrees = 0f;
    private bool _isDragging;
    private Point _lastPos;

    private const float Speed = 0.015f;
    private Shaders.ShaderProgram _shaderProgram;
    private Mesh3D _mesh;
    private bool _isInitialized;
    #endregion

    #region Properties

    public Mesh3D Mesh
    {
        private get => _mesh;
        set
        {
            _mesh = value;
            SetIndices();
        }
    }

    private uint[] _indices = [];
    private int _elementBufferObject;

    private void SetIndices()
    {
        var to = (uint)(Mesh.Triangles.Length * 9);
        _indices=new uint[to];
        for (uint i = 0; i < to; i++) _indices[i] = i;
    }

    private ReadOnlySpan<float> Vertices => Mesh.Vertices();

    #endregion

    public MeshRenderingOpenTkControl(Mesh3D mesh) : base()
    {
        UpdateCameraFront();
        _mesh = mesh;
    }

    public MeshRenderingOpenTkControl() : base()
    {
        UpdateCameraFront();
        _mesh = new([], new(), 0);
    }

    #region overrides

    protected override void OpenTkInit()
    {
        base.OpenTkInit();
        _shaderProgram = new ShaderProgram();
        if (GL.IsBuffer(_vertexBufferObject))
            GL.DeleteBuffer(_vertexBufferObject);
        if (GL.IsVertexArray(_vertexArrayObject))
            GL.DeleteVertexArray(_vertexArrayObject);
        
        _shaderProgram.Use();
        
        //Create vertex and buffer objects
        _vertexArrayObject = GL.GenVertexArray();
        _vertexBufferObject = GL.GenBuffer();

        //Set bg colour to a dark purple
        GL.ClearColor(0.4f, 0f, 0.4f, 1.0f);
        
        //bind to vao
        GL.BindVertexArray(_vertexArrayObject);

        //Set up the buffer for the triangle
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
        
        //Copy triangle vertices to the buffer
        GL.BufferData(BufferTarget.ArrayBuffer,Mesh.Triangles.Length*9*sizeof(float),Vertices.ToArray(),BufferUsageHint.StaticDraw);
        
        //Configure structure of the vertices
        //					  (position parameter in vertex shader, 3 points, data is stored as floats, non-normalized, 5 floats/point, first point at offset 0 in data array)
        GL.VertexAttribPointer(_shaderProgram.GetAttribLocation("aPosition"), 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(_shaderProgram.GetAttribLocation("aPosition"));
        
        //Set up the EBO
        _elementBufferObject = GL.GenBuffer();
        //Set up its buffer
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementBufferObject);
        //Copy data to the buffer
        GL.BufferData(BufferTarget.ElementArrayBuffer, _indices.Length * sizeof(uint), _indices, BufferUsageHint.StaticDraw);
        
        _isInitialized = true;
    }

    //OpenTkRender is called once a frame. The aspect ratio and keyboard state are configured prior to this being called.
    protected override void OpenTkRender()
    {
        base.OpenTkRender();
        GL.Enable(EnableCap.DepthTest);

        //Clear the previous frame
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        //Update camera pos etc
        DoUpdate();

        //Render the object(s)
        DoRender();

        //Clean up the opengl state back to how we got it
        GL.Enable(EnableCap.DepthTest);
    }

    //OpenTkTeardown is called when the control is being destroyed
    protected override void OpenTkTeardown()
    {
        //Bind ArrayBuffer to null so we get an error if any more draw operations go through (helps with debugging)
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        //And ElementArrayBuffer
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

        //Delete our VBO and EBO
        GL.DeleteBuffer(_vertexBufferObject);
        GL.DeleteVertexArray(_vertexArrayObject);

        //Clean up shaders and textures
        _shaderProgram?.Dispose();
        GL.UseProgram(0);
    }

    //Demonstrating use of the Avalonia keyboard state provided by OpenTKAvalonia to control the camera 
    private void DoUpdate()
    {
        var effectiveSpeed = Speed;

        if (KeyboardState.IsKeyDown(Key.LeftShift))
        {
            effectiveSpeed *= 2;
        }

        if (KeyboardState.IsKeyDown(Key.W))
        {
            _cameraPosition += _cameraFront * effectiveSpeed; //Forward 
        }

        if (KeyboardState.IsKeyDown(Key.S))
        {
            _cameraPosition -= _cameraFront * effectiveSpeed; //Backwards
        }

        if (KeyboardState.IsKeyDown(Key.A))
        {
            _cameraPosition -= Vector3.Normalize(Vector3.Cross(_cameraFront, _up)) * effectiveSpeed; //Left
        }

        if (KeyboardState.IsKeyDown(Key.D))
        {
            _cameraPosition += Vector3.Normalize(Vector3.Cross(_cameraFront, _up)) * effectiveSpeed; //Right
        }

        if (KeyboardState.IsKeyDown(Key.Space))
        {
            //Note this is subtracting up, because..? I think avalonia renders the scene upside down.
            _cameraPosition -= _up * effectiveSpeed; //Up 
        }

        if (KeyboardState.IsKeyDown(Key.LeftCtrl))
        {
            _cameraPosition += _up * effectiveSpeed; //Down
        }
    }

    private void DoRender()
    {
        //Bind shaders and textures
        _shaderProgram!.Use();

        //3d projection matrices
        var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_modelRotationDegrees));
        var view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _up);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov),
            (float)(Bounds.Width / Bounds.Height), 0.1f, 100.0f);

        _shaderProgram.SetMatrix4("model", model);
        _shaderProgram.SetMatrix4("view", view);
        _shaderProgram.SetMatrix4("projection", projection);

        //Load configuration from the VAO
        GL.BindVertexArray(_vertexArrayObject);

        //Draw buffer
        GL.DrawArrays(PrimitiveType.Triangles, 0, Mesh.Triangles.Length*9);
    }

    //The following four methods show how to use the Avalonia events for pointer and scroll input to allow moving the camera by clicking-and-dragging and scrolling to zoom
    //It would appear pointer capture doesn't work, at least not as I would expect it to, which is unfortunate
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

        const float sensitivity = 0.05f;

        //Yaw is a function of the change in X
        _yaw -= (float)deltaX * sensitivity;

        //Clamp pitch
        if (_pitch > 89.0f)
        {
            _pitch = 89.0f;
        }
        else if (_pitch < -89.0f)
        {
            _pitch = -89.0f;
        }
        else
        {
            //Pitch is a function of the change in Y
            _pitch += (float)deltaY * sensitivity;
        }

        //Recalculate the camera front vector
        UpdateCameraFront();
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var scrollDelta = e.Delta.Y; //negative is out, positive is in
        _fov -= (float)scrollDelta; //therefore we subtract, because zooming in should decrease the fov
    }

    #endregion


    private void UpdateCameraFront()
    {
        _cameraFront.X = float.Cos(MathHelper.DegreesToRadians(_pitch)) * float.Cos(MathHelper.DegreesToRadians(_yaw));
        _cameraFront.Y = float.Sin(MathHelper.DegreesToRadians(_pitch));
        _cameraFront.Z = -float.Cos(MathHelper.DegreesToRadians(_pitch)) * float.Sin(MathHelper.DegreesToRadians(_yaw));
        _cameraFront = Vector3.Normalize(_cameraFront);
    }
}