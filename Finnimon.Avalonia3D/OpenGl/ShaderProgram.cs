using Finnimon.Math;
using OpenTK.Mathematics;

namespace Finnimon.Avalonia3D.OpenGl;

public class ShaderProgram() : IGlObject
{
    public int Id { get; } = GL.CreateProgram();

    public ShaderProgram(string vertexShaderCode, string fragmentShaderCode) : this() =>
        CompileShaders(Id,vertexShaderCode, fragmentShaderCode);
    
    public static ShaderProgram FromFiles(string basePath)
    =>FromFiles(Path.ChangeExtension(basePath, "vert"),Path.ChangeExtension(basePath, "frag"));
    public static ShaderProgram FromFiles(string vertFile, string fragFile)
        => new (File.ReadAllText(vertFile), File.ReadAllText(fragFile));

    public void Bind() => GL.UseProgram(Id);

    public void Unbind() => GL.UseProgram(0);

    public void Delete() => GL.DeleteShader(Id);

    public static void CompileShaders(int programId ,string vertexShaderCode, string fragmentShaderCode)
    {
        // create the vertex shader
        var vertexShader = CreateShader(vertexShaderCode, ShaderType.VertexShader);
        GL.AttachShader(programId, vertexShader);
        
        var vertInfo= GL.GetShaderInfoLog(vertexShader);
        if (vertInfo is {Length:>0}) Console.Error.WriteLine($"UI: Error compiling fragment shader: {vertInfo}");

        var fragmentShader=CreateShader(fragmentShaderCode, ShaderType.FragmentShader);
        GL.AttachShader(programId, fragmentShader);
        
        var fragInfo= GL.GetShaderInfoLog(fragmentShader);
        if (fragInfo is {Length:>0}) Console.Error.WriteLine($"UI: Error compiling fragment shader: {fragInfo}");

        // Link the program to OpenGL
        GL.LinkProgram(programId);
        GL.DetachShader(programId, vertexShader);
        GL.DetachShader(programId, fragmentShader);
        // delete the shaders
        GL.DeleteShader(vertexShader);
        GL.DeleteShader(fragmentShader);
        Console.WriteLine("Compiled Shaders successfully.");
    }

    public static int CreateShader(string shaderCode, ShaderType shaderType)
    {
        var shaderHandle = GL.CreateShader(shaderType);
        GL.ShaderSource(shaderHandle, shaderCode);
        GL.CompileShader(shaderHandle);
        return shaderHandle;
    }

    public void SetMatrix4(string name, ref Matrix4 matrix)
    {
        var location = GetUniformLocation(name);
        GL.UniformMatrix4(location, false, ref matrix);
    }

    public void SetVec4(string name, in Vector4 vec)
    {
        var location = GetUniformLocation(name);
        GL.Uniform4(location,vec);
    }
    
    public int GetAttribLocation(string attribName) => GL.GetAttribLocation(Id,attribName);
    public int GetUniformLocation(string uniformName) => GL.GetUniformLocation(Id,uniformName);

    ~ShaderProgram() => Delete();

    public void SetVec3(string name, Vector3 vec)
    {
        var location = GetUniformLocation(name);
        GL.Uniform3(location,vec);
    }
}