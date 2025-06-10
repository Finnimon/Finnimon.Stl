using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Finnimon.Avalonia3D;
using Finnimon.Math;

namespace Finnimon.Stl.UI.Simple;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var mesh = Simplmesh();
        // var mesh = ComplexMesh();
        MeshView.RenderModeFlags = RenderMode.Solid;
        MeshView.SetMesh(mesh);
    }

    private static Mesh3D Simplmesh()
    {
        var stl = StlReader.Read(File.OpenRead("./Cube_3d_printing_sample.stl"));
        return new Mesh3D(stl.Facets.Select(facet=>facet.Triangle).ToArray());
    }
    public static Mesh3D ComplexMesh()
    {
        var triangles = StlReader.Read("/home/finnimon/repos/TestFiles/artillery-witch.stl")
            .Facets
            .Select(x=>x.Triangle)
            .ToArray();
        return new Mesh3D(triangles);
    }
    public async void LoadStl(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;

        var topLevel = TopLevel.GetTopLevel(this);
        var storageProvider = topLevel?.StorageProvider ?? throw new PlatformNotSupportedException();

        var options = new FilePickerOpenOptions
        {
            AllowMultiple = false,
            FileTypeFilter = [new FilePickerFileType("STL")],
            SuggestedFileName = "model.stl",
            Title = "Select an STL file"
        };

        var result = await storageProvider.OpenFilePickerAsync(options);
        if (result is not { Count: > 0 }) return;

        try
        {
            await using var stlStream = await result[0].OpenReadAsync();
            var stl = StlReader.Read(stlStream, leaveOpen: false);
            var mesh = ToMesh(stl);
            MeshView.SetMesh(mesh);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static Mesh3D ToMesh(Stl stl)
    {
        var triangles = new Triangle3D[stl.Facets.Count];
        for (var i = 0; i < stl.Facets.Count; i++) triangles[i] = stl.Facets[i].Triangle;
        return new Mesh3D(triangles);
    }

    private void SetRenderMode(object? sender, RoutedEventArgs e)
    {
        if(sender is not MenuItem menu) return;
        e.Handled = true;
        MeshView.RenderModeFlags=menu.Header switch
        {
            nameof(Wireframe)=>RenderMode.WireFrame,
            nameof(Solid)=>RenderMode.Solid,
            nameof(WireframedSolid)=>RenderMode.WireFrame|RenderMode.Solid,
            _=>RenderMode.Solid
        };
    }
}