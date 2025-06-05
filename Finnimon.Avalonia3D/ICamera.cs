using Avalonia;
using Finnimon.Math;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTKAvalonia;

namespace Finnimon.Avalonia3D;


public interface ICamera
{
    public void MoveToSides(float signedMovementScale);
    public void LookAt(Vertex3D target);
    public void MoveForwards(float signedMovementScale);
    public void MoveUp(float signedMovementScalar);
    public (Matrix4 model, Matrix4 view, Matrix4 projection) CreateRenderMatrices(float aspect);

    public (Matrix4 model, Matrix4 view, Matrix4 projection) CreateRenderMatrices(Rect bounds) =>
        CreateRenderMatrices((float)(bounds.Width / bounds.Height));
}

public class OrbitCamera(float fovRad,Vertex3D orbitAround, float distance, float zRotationRad, float cameraPitchRad) : ICamera
{
    #region properties
    public float FovRad { get; set; } = fovRad;
    public Vertex3D OrbitAround { get; private set; }=orbitAround;
    public float Distance { get; private set; } = distance;
    public float ZRotationRad { get; private set; }=zRotationRad;
    public float CameraPitchRad { get; private set; }=cameraPitchRad;
    #endregion
    
    public Vertex3D GetPosition()
    {
        var position=new Vector3(Distance,0,0);
        var pitchMatrix=Matrix3.CreateRotationY(CameraPitchRad);
        var rotationMatrix=Matrix3.CreateRotationZ(ZRotationRad);
        position=position*pitchMatrix*rotationMatrix;
        return position.ToFinnimon()+OrbitAround;
    }

    #region ICamera

    public void MoveToSides(float signedMovementScale) => ZRotationRad+=signedMovementScale;

    public void LookAt(Vertex3D target)=> OrbitAround=target;

    public void MoveForwards(float signedMovementScale) => Distance = System.Math.Max(Distance+signedMovementScale,0.01f);

    public void MoveUp(float signedMovementScalar) => CameraPitchRad += signedMovementScalar;

    public (Matrix4 model, Matrix4 view, Matrix4 projection) CreateRenderMatrices(float aspect)
    {
        var model = Matrix4.Identity;
        var view = Matrix4.LookAt(GetPosition().ToOpenTk(), OrbitAround.ToOpenTk() , Vector3.UnitY);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(FovRad), aspect, 0.1f, 100.0f);
        return (model, view, projection);
    }

    #endregion
}
