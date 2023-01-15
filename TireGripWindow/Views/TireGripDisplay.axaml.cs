using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TireGripWindow.ViewModels;

namespace TireGripWindow.Views;

public partial class TireGripDisplay : Window
{

    public TireGripDisplay()
    {
        InitializeComponent();
    }
    public TireGripDisplay(TireGripViewModel vm)
    {
        DataContext = vm;
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