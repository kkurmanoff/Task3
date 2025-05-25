using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace RocketApp.Views;

public partial class RocketView : UserControl
{
    public RocketView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}