// See https://aka.ms/new-console-template for more information

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

Console.WriteLine($"Starting read of stl {stlFile}");
Stl stl;
try
{
    stl = StlReader.Read(stlFile);
}
catch
{
    Console.WriteLine($"Reading file {stlFile} failed.");
    return 3;
}

Console.WriteLine($"Read {stlFile} in {timer.FormattedTimeSinceLastCall()}.");

_ = stl ?? throw new NullReferenceException(nameof(stl));
var mesh = stl.ToMesh();

Console.WriteLine($"Converted stl to math mesh in {timer.FormattedTimeSinceLastCall()}.");

mesh.InitializeLazies();
var infoMessage =
    $"""
     Stl Info:
        Name          =  {stl.Name ?? ""}
        Header        =  {stl.Header}
        Facet Count   =  {stl.Facets.Count}
        Surface Area  =  {mesh.Area:F2}
        Volume        =  {mesh.Volume:F2}
        Centroids:
             Vertex   =  {mesh.VertexCentroid.Pretty()}
             Area     =  {mesh.AreaCentroid.Pretty()}
             Volume   =  {mesh.VolumeCentroid.Pretty()}

     Successfully evaluated in {timer.FormattedTimeSinceLastCall()}.
     """;
Console.WriteLine(infoMessage);

if (args.Length == 1) return 0;

var writePath = args[1];
var writeModeArg = args.Length >= 3 ? args[2] : "binary";

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
