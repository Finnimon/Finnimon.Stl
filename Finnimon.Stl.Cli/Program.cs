// See https://aka.ms/new-console-template for more information

using Finnimon.Math;
using Finnimon.Stl;
using Finnimon.Stl.Cli;

string[] helpFlags = ["h", "help", "-h", "-help"];
if (args is not { Length: > 0 } || helpFlags.Contains(args[0]))
{
    Console.WriteLine("""
                      Usage: stlcli <stlfilepath> [targetpath] [writemode]");
                      [targetpath] - Use optional targetpath if you want to export the stl elsewhere.");
                      [writemode]  - Use optional writemode if you want to export the stl in a specific mode.
                                     Defaults to 'binary' but may be set to 'ascii' 
                      """);
    return 0;
}

Utils.Timer timer = new();
var stlFile = args[0];
if (!File.Exists(stlFile))
{
    Console.WriteLine($"File \"{stlFile}\" not found.");
    return 2;
}

Stl stl;
try
{
    stl = StlReader.Read(File.OpenRead(stlFile));
}
catch
{
    Console.WriteLine($"Reading file {stlFile} failed.");
    return 3;
}

Console.WriteLine($"Successfully read {stlFile} in {timer.FormattedTimeSinceLastCall()}.");

_ = stl ?? throw new Exception();
var mesh = new Mesh3D(stl.Facets.Select(facet => facet.Triangle).ToArray());

var infoMessage =
    $"""
     Stl Info:
        Name          =  {stl.Name ?? ""}
        Header        =  {stl.Header}
        Facet Count   =  {stl.Facets.Count}
        Surface Area  =  {Mesh3D.CalculateSurfaceArea(mesh.Triangles):F2}
        Volume        =  {mesh.Volume:F2}
        Centroids:    
             Vertex   =  {Mesh3D.CalculateCentroid(mesh.Triangles, MeshCentroidType.Vertex).Pretty()}
             Area     =  {Mesh3D.CalculateCentroid(mesh.Triangles, MeshCentroidType.Area).Pretty()}
             Volume   =  {Mesh3D.CalculateCentroid(mesh.Triangles, MeshCentroidType.Volume).Pretty()}

     Successfully evaluated in {timer.FormattedTimeSinceLastCall()}.
     """;
Console.WriteLine(infoMessage);
if (args.Length == 1) return 0;

var writePath = args[1];
var writeModeArg = args.Length == 3 ? args[2] : null;

var writeMode = writeModeArg switch
{
    "binary" => StlFileFormat.Binary,
    "ascii" => StlFileFormat.Ascii,
    _ => StlFileFormat.Binary,
};

var writeDir = Directory.GetParent(writePath);
if (writeDir is { Exists: false }) writeDir.Create();
StlWriter.Write(stl, File.Create(writePath), writeMode, leaveOpen: false);
Console.WriteLine(
    $"Wrote {Enum.GetName(writeMode)?.ToLower() ?? ""} stl to {writePath} in {timer.FormattedTimeSinceLastCall()}.");
return 0;