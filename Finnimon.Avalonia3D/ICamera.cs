using Avalonia;
using Finnimon.Math;
using OpenTK.Mathematics;

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