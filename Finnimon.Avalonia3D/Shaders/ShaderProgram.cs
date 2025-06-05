using System.Text;
using OpenTK.Graphics.ES11;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using GL = OpenTK.Graphics.OpenGL.GL;
using ShaderType = OpenTK.Graphics.OpenGL.ShaderType;


namespace Finnimon.Avalonia3D.Shaders;

public class ShaderProgram : IDisposable
{
    private readonly int _handle;
    private bool _disposedValue;

    public ShaderProgram(string vert="Shaders/default.vert", string frag="Shaders/default.frag")
    {
        var vertexShaderSource = File.ReadAllText(vert, Encoding.UTF8);
        var fragmentShaderSource = File.ReadAllText(frag, Encoding.UTF8);

        //Create GL shaders for the shader source files
        var vertexShader = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(vertexShader, vertexShaderSource);

        var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(fragmentShader, fragmentShaderSource);
			
        //Compile and error-check the vertex shader
        GL.CompileShader(vertexShader);

         GL.GetShaderInfoLog(vertexShader,out var infoLogVert);
        if (infoLogVert != string.Empty) Console.Error.WriteLine($"UI: Error compiling vertex shader: {infoLogVert}");

        //Compile and error-check the fragment shader
        GL.CompileShader(fragmentShader);

        GL.GetShaderInfoLog(fragmentShader,out var infoLogFrag);

        if (infoLogFrag != string.Empty) Console.Error.WriteLine($"UI: Error compiling fragment shader: {infoLogFrag}");

        //Create a GL program, and attach shaders
        _handle = GL.CreateProgram();
        GL.AttachShader(_handle, vertexShader);
        GL.AttachShader(_handle, fragmentShader);
			
        //Link
        GL.LinkProgram(_handle);
			
        //Cleanup
        GL.DetachShader(_handle, vertexShader);
        GL.DetachShader(_handle, fragmentShader);
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
			
        Console.WriteLine("UI: Compiled shaders successfully");
    }
    public void Use()
    {
        GL.UseProgram(_handle);
    }
    public int GetAttribLocation(string attribName) => GL.GetAttribLocation(_handle, attribName);

    public int GetUniformLocation(string uniformName) => GL.GetUniformLocation(_handle, uniformName);

    public void SetInt(string name, int value) => GL.Uniform1(GetUniformLocation(name), value);

    public void SetMatrix4(string name, Matrix4 value) 
        => GL.UniformMatrix4(GL.GetUniformLocation(_handle, name), false, ref value);

    ~ShaderProgram()
    {
        if(GL.IsProgram(_handle)) GL.DeleteProgram(_handle);
    }
		
    public void Dispose()
    {
        if (_disposedValue)
        {
            return;
        }

        GL.DeleteProgram(_handle);

        _disposedValue = true;
			
        GC.SuppressFinalize(this);
    }
}