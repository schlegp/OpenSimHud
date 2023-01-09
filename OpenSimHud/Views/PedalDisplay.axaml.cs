using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace OpenSimRadar.Views;

public partial class PedalDisplay : Window
{
    public PedalDisplay()
    {
        InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}