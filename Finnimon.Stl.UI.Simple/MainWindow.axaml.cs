using Avalonia.Controls;
using Finnimon.Avalonia3D;
using Finnimon.Math;

namespace Finnimon.Stl.UI.Simple;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // var triangles = StlReader.Read("/home/finnimon/repos/Finnimon.Stl/Finnimon.Stl.Benchmark/Files/big.stl")
        //     .Facets
        //     .Select(x=>x.Triangle)
        //     .ToArray();
        // var mesh = new Mesh3D(triangles);
        var mesh = Mesh;
        MeshView.SetMesh(mesh);
    }

    private static Mesh3D Simplmesh()
    {
        var triangle = new Triangle3D(new(Y: 1), new(Z: 1), new(Y: -1));
        return new([triangle]);
    }
    public static Mesh3D Mesh
    {
        get {
            var stl = StlReader.Read(File.OpenRead("./Cube_3d_printing_sample.stl"));
            return new Mesh3D(stl.Facets.Select(facet=>facet.Triangle).ToArray());
        }
    }
}