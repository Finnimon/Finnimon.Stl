using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Finnimon.Avalonia3D;
using Finnimon.Math;

namespace Finnimon.Stl.UI.Simple;


public partial class MainWindow : Window
{
    private Stl? _stl;

    private Stl? Stl
    {
        get => _stl;
        set
        {
            _stl = value;
            var mesh =value?.ToMesh()??new([]);
            MeshView.Mesh = mesh;
            UpdateBlocks(mesh);
        }
    }

    

    public MainWindow()
    {
        InitializeComponent();
        Stl = Cube();
        BackgroundColorPicker.Color = MeshView.AvaloniaBackgroundColor;
        WireframeColorPicker.Color = MeshView.AvaloniaWireframeColor;
        SolidColorPicker.Color = MeshView.AvaloniaSolidColor;
    }

    private void UpdateBlocks(Mesh3D mesh)
    {
        var (_, centroid, volume, area) = mesh;
        CentroidBlock.Text = $"Centroid: X{centroid.X:N2} Y{centroid.Y:N2} Z{centroid.Z:N2}";
        VolumeBlock.Text = $"Volume: {volume:N2}";
        SurfBlock.Text=$"Surface area: {area:N2}";
    }

    private static Stl Cube() => StlReader.Read("./Cube_3d_printing_sample.stl");

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
            Stl = StlReader.Read(stlStream, leaveOpen: false);
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
            nameof(Wireframe)=>RenderMode.Wireframe,
            nameof(Solid)=>RenderMode.Solid,
            nameof(WireframedSolid)=>RenderMode.Wireframe|RenderMode.Solid,
            _=>RenderMode.Solid
        };
    }

    private void ExportBinary(object? sender, RoutedEventArgs e)
        => ExportCommand(e, StlFileFormat.Binary);
    private void ExportAscii(object? sender, RoutedEventArgs e)
        => ExportCommand(e, StlFileFormat.Ascii);

    private async void ExportCommand(RoutedEventArgs routedEventArgs, StlFileFormat saveFormat)
    {
        routedEventArgs.Handled = true;
        if(Stl is null) return;
        
        var topLevel = GetTopLevel(this);
        var storageProvider = topLevel?.StorageProvider ?? throw new PlatformNotSupportedException();

        var options = new FilePickerSaveOptions()
        {
            DefaultExtension = ".stl",
            SuggestedFileName = "model.stl"
        };
        var fileSaver = await storageProvider.SaveFilePickerAsync(options);
        if(fileSaver is null) return;
        try
        {
            await fileSaver.DeleteAsync();
            await using var writeStream = await fileSaver.OpenWriteAsync();
            StlWriter.Write(Stl,writeStream,saveFormat,leaveOpen: true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    [Obsolete]
    private async void OpenColorSettings(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        MeshViewColorPickerWindow? dialog=null;
        try
        {
            dialog=new MeshViewColorPickerWindow(MeshView.AvaloniaBackgroundColor,MeshView.AvaloniaWireframeColor,MeshView.AvaloniaSolidColor);
            await dialog.ShowDialog(this);
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        if (dialog is not { Canceled: false }) return;
        MeshView.AvaloniaBackgroundColor=dialog.BackgroundColor;
        MeshView.AvaloniaWireframeColor=dialog.WireframeColor;
        MeshView.AvaloniaSolidColor=dialog.SolidColor;

    }

    private void ColorSettingsChanged(object? sender, ColorChangedEventArgs e)
    {
        if(BackgroundColorPicker.Equals(sender)) MeshView.AvaloniaBackgroundColor = e.NewColor;
        else if(WireframeColorPicker.Equals(sender)) MeshView.AvaloniaWireframeColor = e.NewColor;
        else if(SolidColorPicker.Equals(sender)) MeshView.AvaloniaSolidColor = e.NewColor;
    }
}

