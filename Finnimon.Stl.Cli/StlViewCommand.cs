using System.Runtime.InteropServices;
using System.Diagnostics;
using Spectre.Console.Cli;
using GuiProgram = Finnimon.Stl.UI.Simple.Program;
namespace Finnimon.Stl.Cli;

public sealed class StlViewCommand : Command<StlViewSettings>
{
    public override int Execute(CommandContext context, StlViewSettings settings)
    {
        var stl = settings.StlPath ?? string.Empty;
        if (stl is { Length: > 0 } && !File.Exists(stl))
        {
            Console.WriteLine($"File not found: {stl}");
            return 1;
        }
        try
        {
            RunDisownedProcess(GetExecutable(), stl);
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return 1;
        }
    }
    private static string GetExecutable()
    {
        var path = typeof(GuiProgram).Assembly.Location;
        var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "exe"
                : null;
        return Path.ChangeExtension(path, ext);
    }
    private static void RunDisownedProcess(string executable, string args)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = args,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            Process.Start(startInfo);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                 RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Wrap command in a shell so we can run it disowned (`&`), redirect output, and detach
            string shell = "/bin/bash";
            string shellArgs = $"-c \"nohup {EscapeShellArg(executable)} {args} > /dev/null 2>&1 &\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = shellArgs,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                RedirectStandardInput = false
            };

            Process.Start(startInfo);
        }
        else
        {
            throw new PlatformNotSupportedException("Unsupported OS platform");
        }
    }

    private static string EscapeShellArg(string arg)
    {
        // Escape only if needed
        return arg.Contains(" ") ? $"\"{arg}\"" : arg;
    }
}
