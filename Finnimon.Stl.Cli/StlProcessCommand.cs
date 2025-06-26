using Spectre.Console;
using Spectre.Console.Cli;
using Finnimon.Stl;
namespace Finnimon.Stl.Cli;

public class StlProcessCommand : AsyncCommand<StlProcessSettings>
{
    public override async Task<int> ExecuteAsync(CommandContext context, StlProcessSettings settings)
    {
        Utils.Timer timer = new();

        if (!File.Exists(settings.StlPath))
        {
            AnsiConsole.MarkupLine($"[red]Error:[/] File [yellow]{settings.StlPath}[/] not found.");
            return 2;
        }

        AnsiConsole.MarkupLine($"[green]Reading STL:[/] {settings.StlPath}");

        Stl stl;
        try
        {
            stl = StlReader.Read(settings.StlPath);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to read STL file:[/] {settings.StlPath}");
            AnsiConsole.WriteLine(ex.ToString());
            return 3;
        }

        AnsiConsole.MarkupLine($"[blue]Read in:[/] {timer.FormattedTimeSinceLastCall()}");

        var mesh = stl.ToMesh();
        AnsiConsole.MarkupLine($"[green]Converted to mesh in:[/] {timer.FormattedTimeSinceLastCall()}");

        await mesh.InitializeLaziesParallel();

        AnsiConsole.MarkupLine(
            $"""
            [bold yellow]STL Info:[/]
              [blue]Name         :[/] {stl.Name ?? ""}
              [blue]Header       :[/] {stl.Header}
              [blue]Facet Count  :[/] {stl.Facets.Count}
              [blue]Surface Area :[/] {mesh.Area:F2}
              [blue]Volume       :[/] {mesh.Volume:F2}
              [blue]Centroids:[/]
                Vertex: {mesh.VertexCentroid.Pretty()}
                Area  : {mesh.AreaCentroid.Pretty()}
                Volume: {mesh.VolumeCentroid.Pretty()}
              [green]Completed in:[/] {timer.FormattedTimeSinceLastCall()}
            """);

        if (settings.TargetPath is not null)
        {
            var writeMode = settings.WriteMode.ToLower() switch
            {
                "ascii" => StlFileFormat.Ascii,
                "binary" => StlFileFormat.Binary,
                _ => StlFileFormat.Binary,
            };

            var writeDir = Path.GetDirectoryName(settings.TargetPath);
            if (!string.IsNullOrEmpty(writeDir) && !Directory.Exists(writeDir))
            {
                Directory.CreateDirectory(writeDir);
            }

            using var fs = File.Create(settings.TargetPath);
            StlWriter.Write(stl, fs, writeMode, leaveOpen: false);

            AnsiConsole.MarkupLine(
                $"[green]Wrote [bold]{writeMode}[/] STL to [underline]{settings.TargetPath}[/] in {timer.FormattedTimeSinceLastCall()}.[/]");
        }

        return 0;
    }
}
