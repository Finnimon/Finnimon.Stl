using Finnimon.Math;
using OpenTK.Mathematics;

namespace Finnimon.Avalonia3D;

public class OrbitCamera(float fovRad, Vertex3D orbitAround, float distance, float azimuthRad, float pitchRad) : ICamera
{
    private Vertex3D _unitUp = Vertex3D.ZAxis;
    public float FovRad { get; set; } = fovRad;
    public Vertex3D OrbitAround { get; set; } = orbitAround;
    public float Distance { get; set; } = distance;

    public Vertex3D UnitUp
    {
        get => _unitUp;
        set => _unitUp = value.Normalize();
    }

    public float AzimuthRad { get; set; } = azimuthRad; // around UnitUp
    public float PitchRad { get; set; } = pitchRad;     // around CameraRight

    public Vertex3D GetPosition()
    {
        // Ensure UnitUp is normalized
        var up = UnitUp.Normalize();

        // Find an arbitrary vector not parallel to Up
        var reference = (MathF.Abs(Vector3.Dot(up.ToOpenTk(), Vector3.UnitY)) < 0.99f)
            ? Vertex3D.YAxis
            : Vertex3D.XAxis;

        // Construct a right-handed orthonormal basis:
        var right = (reference^up).Normalize();
        var forward = (up^right).Normalize();

        // Spherical coordinate offset from center
        var local =
            right   * (Distance * MathF.Cos(PitchRad) * MathF.Cos(AzimuthRad)) +
            forward * (Distance * MathF.Cos(PitchRad) * MathF.Sin(AzimuthRad)) +
            up      * (Distance * MathF.Sin(PitchRad));

        return OrbitAround + local;
    }

    public void MoveToSides(float signedMovementScale) => AzimuthRad += signedMovementScale;

    public void MoveUp(float signedMovementScalar) => PitchRad = MathHelper.Clamp(PitchRad + signedMovementScalar, -MathF.PI / 2f + 0.01f, MathF.PI / 2f - 0.01f);

    public void MoveForwards(float signedMovementScale) => Distance = MathF.Max(Distance - signedMovementScale, 0.01f);

    public void LookAt(Vertex3D target) => OrbitAround = target;

    public (Matrix4 model, Matrix4 view, Matrix4 projection) CreateRenderMatrices(float aspect)
    {
        var model = Matrix4.Identity;
        var view = Matrix4.LookAt(GetPosition().ToOpenTk(), OrbitAround.ToOpenTk(), UnitUp.ToOpenTk());
        var projection = Matrix4.CreatePerspectiveFieldOfView(FovRad, aspect, 0.001f, Distance*4);
        return (model, view, projection);
    }
}
