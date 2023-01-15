using OpenSimHud.Basics.ViewModels;
using ReactiveUI;

namespace TireGripWindow.ViewModels;

public class TireGripViewModel : ViewModelBase
{
    private double _tireGripFl;
    public double TireGripFl
    {
        get => _tireGripFl;
        set => this.RaiseAndSetIfChanged(ref _tireGripFl, value);
    }
    
    private double _tireGripFr;
    public double TireGripFr
    {
        get => _tireGripFr;
        set => this.RaiseAndSetIfChanged(ref _tireGripFr, value);
    }
    
    private double _tireGripRl;
    public double TireGripRl
    {
        get => _tireGripRl;
        set => this.RaiseAndSetIfChanged(ref _tireGripRl, value);
    }
    
    private double _tireGripRr;
    public double TireGripRr
    {
        get => _tireGripRr;
        set => this.RaiseAndSetIfChanged(ref _tireGripRr, value);
    }
}