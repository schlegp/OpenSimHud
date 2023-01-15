using OpenSimHud.Basics.ViewModels;
using ReactiveUI;

namespace RadarWindow.ViewModels;

public class RadarDisplayViewModel : ViewModelBase
{
    private double _canvasRotation = 0;
    private double _canvasX = 0;
    private double _canvasY = 0;
    private double _canvasScale = 1;
    private double _windowSize = 300;
    private double _closestCarDistance = 0;
    
    public double CanvasRotation
    {
        get => _canvasRotation; 
        set => this.RaiseAndSetIfChanged(ref _canvasRotation, value); 
    }
    
    public double CanvasX
    {
        get => _canvasX; 
        set => this.RaiseAndSetIfChanged(ref _canvasX, value); 
    }
    
    public double CanvasY
    {
        get => _canvasY; 
        set => this.RaiseAndSetIfChanged(ref _canvasY, value); 
    }

    public double CanvasScale
    {
        get => _canvasScale;
        set => this.RaiseAndSetIfChanged(ref _canvasScale, value);
    }
    public double WindowSize
    {
        get => _windowSize;
        set => this.RaiseAndSetIfChanged(ref _windowSize, value);
    }
    
    public double ClosestCarDistance
    {
        get => _closestCarDistance;
        set => this.RaiseAndSetIfChanged(ref _closestCarDistance, value);
    }
}