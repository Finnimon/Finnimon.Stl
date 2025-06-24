using Spectre.Console.Cli;
using System.ComponentModel;

namespace Finnimon.Stl.Cli;

public class StlViewSettings : CommandSettings
{
    [CommandArgument(0, "[stlfilepath]")]
    [Description("Path to the STL file to view.")]
    public string? StlPath { get; set; } = null;
}
