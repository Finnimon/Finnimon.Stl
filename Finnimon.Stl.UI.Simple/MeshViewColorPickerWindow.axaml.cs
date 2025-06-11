using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Finnimon.Stl.UI.Simple;

[Obsolete]
public partial class MeshViewColorPickerWindow : Window
{
    public bool Canceled = true;
    public Color BackgroundColor => BackgroundColorPicker.Color;
    public Color WireframeColor => WireframeColorPicker.Color;
    public Color SolidColor => SolidColorPicker.Color;
    public MeshViewColorPickerWindow(Color bgColor, Color wfColor,Color solidColor)
    {
        InitializeComponent();
        BackgroundColorPicker.Color=bgColor;
        WireframeColorPicker.Color=wfColor;
        SolidColorPicker.Color=solidColor;
    }


    private void OkClicked(object? sender, RoutedEventArgs e)
    {
        Canceled = false;
        Close();
    }

    private void CancelClicked(object? sender, RoutedEventArgs e) => Close();
}