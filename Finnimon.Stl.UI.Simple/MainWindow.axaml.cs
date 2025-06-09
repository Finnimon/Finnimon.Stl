using Avalonia.Controls;
using Finnimon.Avalonia3D;
using Finnimon.Math;

namespace Finnimon.Stl.UI.Simple;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // var mesh = Simplmesh();
        var mesh = ComplexMesh();
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
}