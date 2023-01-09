using ReactiveUI;

namespace OpenSimHud.ViewModels;

public class PedalDisplayViewModel: ViewModelBase
{
    private double _throttleValue;
    
    public double ThrottleValue
    {
        get => _throttleValue; 
        set => this.RaiseAndSetIfChanged(ref _throttleValue, value); 
    }
    
    private double _brakeValue;
    
    public double BrakeValue
    {
        get => _brakeValue; 
        set => this.RaiseAndSetIfChanged(ref _brakeValue, value); 
    }
}