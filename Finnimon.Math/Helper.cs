using System.Numerics;

namespace Finnimon.Math;

public static class Helper
{
    private const float PiOver180 = float.Pi / 180;
    public static float DegToRad(this float degrees) => degrees*float.Pi/180;
    public static float RadToDeg(this float radians) => radians/PiOver180;
}