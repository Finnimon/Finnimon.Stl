using Finnimon.Math;
using OpenTK.Mathematics;

namespace Finnimon.Avalonia3D;

public class OrbitCamera(float fovRad, Vertex3D orbitAround, float distance, float azimuthRad, float pitchRad) : ICamera
{
    public float FovRad { get; set; } = fovRad;
    public Vertex3D OrbitAround { get; private set; } = orbitAround;
    public float Distance { get; private set; } = distance;

    public float AzimuthRad { get; private set; } = azimuthRad; // around Z
    public float PitchRad { get; private set; } = pitchRad;     // around local X

    public Vertex3D GetPosition()
    {
        // Spherical coordinates in Z-up system
        float x = Distance * MathF.Cos(PitchRad) * MathF.Cos(AzimuthRad);
        float y = Distance * MathF.Cos(PitchRad) * MathF.Sin(AzimuthRad);
        float z = Distance * MathF.Sin(PitchRad);

        return new Vertex3D(x, y, z) + OrbitAround;
    }

    public void MoveToSides(float signedMovementScale) => AzimuthRad += signedMovementScale;

    public void MoveUp(float signedMovementScalar) => PitchRad = MathHelper.Clamp(PitchRad + signedMovementScalar, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);

    public void MoveForwards(float signedMovementScale) => Distance = MathF.Max(Distance - signedMovementScale, 0.01f);

    public void LookAt(Vertex3D target) => OrbitAround = target;

    public (Matrix4 model, Matrix4 view, Matrix4 projection) CreateRenderMatrices(float aspect)
    {
        var model = Matrix4.Identity;
        var view = Matrix4.LookAt(GetPosition().ToOpenTk(), OrbitAround.ToOpenTk(), Vector3.UnitZ);
        var projection = Matrix4.CreatePerspectiveFieldOfView(FovRad, aspect, 0.001f, Distance*4);
        return (model, view, projection);
    }
}
