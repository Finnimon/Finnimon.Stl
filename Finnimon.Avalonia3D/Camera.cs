using Avalonia;
using OpenTK.Mathematics;

namespace Finnimon.Avalonia3D;

public class Camera
{
    public Vector3 _cameraPosition = new(80, 0, 0);
    public Vector3 _cameraFront=Vector3.UnitX;
    public Vector3 _up = Vector3.UnitZ;
    public float _fov = 45;
    public float _pitch = -40;
    public float _yaw = 90f;
    public float _modelRotationDegrees = 0f;
    public float Speed=0.015f;

    public (Matrix4 model, Matrix4 view, Matrix4 projection) GetMatrices(Rect bounds)
    {
        var model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_modelRotationDegrees));
        var view = Matrix4.LookAt(_cameraPosition, _cameraPosition + _cameraFront, _up);
        var projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(_fov),
            (float)(bounds.Width / bounds.Height), 0.1f, 100.0f);
        return (model, view, projection);
    }
    
    
    public void UpdateCameraFront()
    {
        _cameraFront.X = float.Cos(MathHelper.DegreesToRadians(_pitch)) * float.Cos(MathHelper.DegreesToRadians(_yaw));
        _cameraFront.Y = float.Sin(MathHelper.DegreesToRadians(_pitch));
        _cameraFront.Z = -float.Cos(MathHelper.DegreesToRadians(_pitch)) * float.Sin(MathHelper.DegreesToRadians(_yaw));
        _cameraFront = Vector3.Normalize(_cameraFront);
    }
    
    public void LookAt(Vector3 target)
    {
        var direction = Vector3.Normalize(target - _cameraPosition);
    
        _pitch = MathHelper.RadiansToDegrees(MathF.Asin(direction.Y));
        _yaw = MathHelper.RadiansToDegrees(MathF.Atan2(-direction.Z, direction.X));
    
        UpdateCameraFront();
    }
}