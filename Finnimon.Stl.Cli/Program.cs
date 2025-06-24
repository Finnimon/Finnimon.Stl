using Spectre.Console.Cli;
using Finnimon.Stl.Cli;
var app = new CommandApp<StlProcessCommand>();

app.Configure(config =>
{
    config.SetApplicationName("stlcli");
    config.ValidateExamples();
    config.AddCommand<StlProcessCommand>("process")
          .WithDescription("Processes and optionally rewrites an STL file.")
          .WithExample(["process", "./example.stl", "./output.stl", "ascii"]);
    config.AddCommand<StlViewCommand>("view")
        .WithDescription("View an STL file with opengl based rendering.")
        .WithExample(["view","./example.stl"]);
});

return await app.RunAsync(args);
