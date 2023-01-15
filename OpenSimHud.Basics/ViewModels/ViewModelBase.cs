using ReactiveUI;

namespace OpenSimHud.Basics.ViewModels;

public class ViewModelBase : ReactiveObject
{
    
    
    private List<VehicleGPS> _cars = new ();
    
    public List<VehicleGPS> Cars
    {
        get => _cars;
        set => this.RaiseAndSetIfChanged(ref _cars, value);
    }
}