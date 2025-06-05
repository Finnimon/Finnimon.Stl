using System.Runtime.CompilerServices;

namespace Finnimon.Math;

public static class IntExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool InsideInclusiveRange(this int value, int min, int max)
    =>(uint)(value - min) <= (uint)(max - min);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool OutsideInclusiveRange(this int value, int min, int max)
        =>(uint)(value - min) > (uint)(max - min);
}