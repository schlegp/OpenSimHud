using System.ComponentModel;
using System.Globalization;
using System.Numerics;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using DynamicData;
using RadarWindow.Converters;
using RadarWindow.Models;
using RadarWindow.ViewModels;

namespace RadarWindow.Views;

public partial class RadarDisplay : Window
{
    private readonly DispatcherTimer _renderTimer;

    private readonly DistanceToColorConverter _converter = DistanceToColorConverter.Instance;

    public RadarDisplayViewModel DisplayViewModel { get; set; }
    
    /// <summary>
    /// Tracks cars available on the Canvas
    /// </summary>
    private readonly List<CarImage> _canvasCars;

    private readonly Vector3 _carCenterVector = new()
        {X = (float)CarLength / 2, Y = 0, Z = (float)CarWidth / 2};

    private class CarImage : Rectangle //Image
    {
        public int Id { get; set; }
        public double DistanceToPlayer { get; set; }
    }

    private readonly List<CarPosition> _carPositions = new();
    private CarImage _playerCar;

    public RadarDisplay()
    {
        InitializeComponent();
    }
    
    public RadarDisplay(RadarDisplayViewModel radarViewModel)
    {
        this.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
        this.AddHandler(PointerWheelChangedEvent, ScrollHandler, handledEventsToo: true);

        // _renderTimer = new DispatcherTimer();
        // _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
        // _renderTimer.Tick += RenderCycle;

        DisplayViewModel = radarViewModel;

        _canvasCars = new();
        DisplayViewModel.Cars = new();
        _playerCar = new CarImage();

        // DisplayViewModel.Cars.CollectionChanged += RenderCycle;
        DisplayViewModel.PropertyChanged += ViewModelChangedHandler;
        
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
    }
    private void ViewModelChangedHandler(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DisplayViewModel.Cars))
        {
            RenderCycle();
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        RadarCanvas = this.FindControl<Canvas>("RadarCanvas");

        // _renderTimer.Start();
    }
    
    private static double CarWidth => 2 * 10;
    private static double CarLength => 4 * 10;
    
    private void MoveCanvas()
    {
        var x = _playerCar.GetValue(Canvas.LeftProperty);
        var y = _playerCar.GetValue(Canvas.TopProperty);
        var orientation = (_playerCar.RenderTransform as RotateTransform)?.Angle ?? 0;
        var allCars = DisplayViewModel.Cars?.Where(car => !car.IsPlayer).ToArray();
        var closestDistance = allCars != null && allCars.Any() ? allCars.Min(car => car.DistanceToPlayer) : 0;
        var opacity =
            (double?)DistanceToOpacityConverter.Instance.Convert(closestDistance, typeof(double), 0.0,
                CultureInfo.CurrentCulture);

        // Console.WriteLine($"Closest car is: {opacity*100}%");
        
        DisplayViewModel.CanvasRotation = -orientation;
        DisplayViewModel.CanvasX = -x + (Width / 2);
        DisplayViewModel.CanvasY = -y + (Height / 2);
        DisplayViewModel.ClosestCarDistance = opacity ?? 1.0;
    }
    
    private void RenderCycle(object? sender, EventArgs e)
    {
        RenderCycle();
    }

    private void RenderCycle()
    {
        if (!DisplayViewModel.Cars.Any())
        {
            return;
        }
        CheckForNewCars();
        CheckForMissingCars();
        UpdateCarPositions();

        MoveCanvas();
    }
    
    private void CheckForMissingCars()
    {
        var carsToRemove = _canvasCars.Where(x => DisplayViewModel.Cars.All(c => c.Id != x.Id)).ToArray();
        _carPositions.RemoveAll(x => carsToRemove.Any(c => c.Id == x.Id));
        ListEx.RemoveMany<IControl>(RadarCanvas.Children, carsToRemove);
        _canvasCars.RemoveMany(carsToRemove);
    }

    private void UpdateCarPositions()
    {
        foreach (var car in _carPositions)
        {
            var gameCar = DisplayViewModel.Cars.FirstOrDefault(c => c.Id == car.Id);
            if (gameCar != default)
            {
                car.Distance.OnNext(gameCar.DistanceToPlayer);
                car.X.OnNext(gameCar.Position.X);
                car.Y.OnNext(gameCar.Position.Z);
                car.Orientation.OnNext(gameCar.Orientation + 90);
            }
        }
    }
    
    private void ScrollHandler(object? sender, PointerWheelEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            DisplayViewModel.CanvasScale += (0.1 * e.Delta.Y);
        }

        if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            DisplayViewModel.WindowSize += (10 * e.Delta.X);
        }
    }
    
    private void CheckForNewCars()
    {
        foreach (var car in DisplayViewModel.Cars.Where(c => _canvasCars.All(x => x.Id != c.Id)))
        {
            var carPosition = new CarPosition()
            {
                Id = car.Id
            };

            var image = new CarImage()
            {
                Id = car.Id,
                DistanceToPlayer = car.DistanceToPlayer,
                Width = CarWidth,
                Height = CarLength,
                // Offset positions so the Rectangle is centered
                [!Canvas.LeftProperty] = carPosition.X.Select(x => (x * 10) + _carCenterVector.X).ToBinding(),
                [!Canvas.TopProperty] = carPosition.Y.Select(y => (y * 10) + _carCenterVector.Z).ToBinding(),
                [!Shape.FillProperty] = carPosition.Distance.Select(x => _converter.Convert(x)).ToBinding(),
            };
            var carRotation = new RotateTransform();
            carRotation.Bind(RotateTransform.AngleProperty, carPosition.Orientation.Select(x => x).ToBinding());
            image.RenderTransform = carRotation;
            if (car.IsPlayer)
            {
                _playerCar = image;
            }
            RadarCanvas.Children.Add(image);
            _carPositions.Add(carPosition);
            _canvasCars.Add(image);
        }
    }
    
    private void MouseDownHandler(object? sender, PointerPressedEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
        {
            this.BeginMoveDrag(e);
        }
    }
}