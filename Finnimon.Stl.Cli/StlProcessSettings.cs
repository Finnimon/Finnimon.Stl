using Spectre.Console.Cli;
using System.ComponentModel;
using Finnimon.Stl;
namespace Finnimon.Stl.Cli;
public class StlProcessSettings : CommandSettings
{
    [CommandArgument(0, "<stlfilepath>")]
    [Description("Path to the STL file to process.")]
    public string StlPath { get; set; } = default!;

    [CommandArgument(1, "[targetpath]")]
    [Description("Optional output path to export the STL file.")]
    public string? TargetPath { get; set; }

    [CommandArgument(2, "[writemode]")]
    [Description("Optional write mode: binary (default) or ascii.")]
    public string WriteMode { get; set; } = "binary";
}
