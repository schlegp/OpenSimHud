using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using PedalWindow.ViewModels;

namespace PedalWindow.Views;

public partial class PedalDisplay : Window
{

    public PedalDisplay()
    {
        InitializeComponent();
    }
    public PedalDisplay(PedalDisplayViewModel vm)
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