using System.Diagnostics;
using Finnimon.Math;

namespace Finnimon.Stl.Cli;


public static class Utils
{
    public class Timer
    {
        private Stopwatch _stopwatch=Stopwatch.StartNew();
        public string FormattedTimeSinceLastCall()
        {
            var msg = $"{_stopwatch.ElapsedMilliseconds}ms";
            _stopwatch.Restart();
            return msg;
        }
    }
    
    public static string Pretty(this Vertex3D vert)
    =>$"X={vert.X:F2} Y={vert.Y:F2} Z={vert.Z:F2}";
}