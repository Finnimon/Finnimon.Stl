namespace Finnimon.Math;

public static class DoubleExt
{
    public static bool Approx(this double me, double other, double epsilon=double.Epsilon) => System.Math.Abs(me - other) < epsilon;
    public static double Abs(this double me) => me < 0.0 ? -me : me;
    public static bool Approx(this float me, float other, float epsilon=float.Epsilon) => System.Math.Abs(me - other) < epsilon;
}