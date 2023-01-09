using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
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
using OpenSimHud.Converters;
using OpenSimHud.Models;
using OpenSimHud.ViewModels;
using OpenSimRadar.Views;
using rF2SMLib;
using rF2SMLib.DTOs;

namespace OpenSimHud.Views;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _renderTimer;

    private readonly BackgroundWorker _backgroundWorker;

    private readonly DistanceToColorConverter _converter = DistanceToColorConverter.Instance;

    private readonly TelemetryReceiver _receiver;

    private PedalData _currentPedalData = new PedalData();

    /// <summary>
    /// Cars coming from Game
    /// </summary>
    private List<VehicleGPS> _cars;

    /// <summary>
    /// Tracks cars available on the Canvas
    /// </summary>
    private readonly List<CarImage> _canvasCars;

    private readonly PedalDisplayViewModel _pedalDisplayViewModel;
    private readonly PedalDisplay _pedalDisplay;

    private readonly Vector3 _carCenterVector = new()
        {X = (float)CarLength / 2, Y = 0, Z = (float)CarWidth / 2};

    private class CarImage : Rectangle //Image
    {
        public int Id { get; set; }
        public double DistanceToPlayer { get; set; }
    }

    private readonly List<CarPosition> _carPositions = new();
    private CarImage _playerCar;

    public MainWindow()
    {
        _receiver = new TelemetryReceiver();
        this.AddHandler(PointerPressedEvent, MouseDownHandler, handledEventsToo: true);
        this.AddHandler(PointerWheelChangedEvent, ScrollHandler, handledEventsToo: true);

        _renderTimer = new DispatcherTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
        _renderTimer.Tick += RenderCycle;

        _backgroundWorker = new BackgroundWorker();
        _backgroundWorker.DoWork += GetTelemetry;

        _canvasCars = new();
        _cars = new List<VehicleGPS>();
        _playerCar = new CarImage();
        InitializeComponent();

        _pedalDisplay = new PedalDisplay();
        _pedalDisplayViewModel = new PedalDisplayViewModel();
        
        _pedalDisplay.DataContext = _pedalDisplayViewModel;
        
        _pedalDisplay.Show();
    }
    private void GetTelemetry(object? sender, DoWorkEventArgs e)
    {
        _cars = _receiver.GetNextCarTelemetry().Item1;
        _currentPedalData = _receiver.GetNextCarTelemetry().Item2;
    }
    private void RenderCycle(object? sender, EventArgs e)
    {
        if (!_backgroundWorker.IsBusy)
        {
            _backgroundWorker.RunWorkerAsync();
        }

        CheckForNewCars();
        CheckForMissingCars();
        UpdateCarPositions();

        MoveCanvas();
        
        // Update Pedals
        _pedalDisplayViewModel.BrakeValue = _currentPedalData.BrakePedal * _pedalDisplay.Height;
        _pedalDisplayViewModel.ThrottleValue = _currentPedalData.Throttle * _pedalDisplay.Height;
    }
    private void MoveCanvas()
    {
        var x = _playerCar.GetValue(Canvas.LeftProperty);
        var y = _playerCar.GetValue(Canvas.TopProperty);
        var orientation = (_playerCar.RenderTransform as RotateTransform)?.Angle ?? 0;
        var allCars = _cars?.Where(car => !car.IsPlayer).ToArray();
        var closestDistance = allCars != null && allCars.Any() ? allCars.Min(car => car.DistanceToPlayer) : 0;
        var opacity =
            (double?)DistanceToOpacityConverter.Instance.Convert(closestDistance, typeof(double), 0.0,
                CultureInfo.CurrentCulture);

        // Console.WriteLine($"Closest car is: {opacity*100}%");
        
        ((MainWindowViewModel)DataContext).CanvasRotation = -orientation;
        ((MainWindowViewModel)DataContext).CanvasX = -x + (Width / 2);
        ((MainWindowViewModel)DataContext).CanvasY = -y + (Height / 2);
        ((MainWindowViewModel)DataContext).ClosestCarDistance = opacity ?? 1.0;
    }

    private void UpdateCarPositions()
    {
        foreach (var car in _carPositions)
        {
            var gameCar = _cars.FirstOrDefault(c => c.Id == car.Id);
            if (gameCar != default)
            {
                car.Distance.OnNext(gameCar.DistanceToPlayer);
                car.X.OnNext(gameCar.Position.X);
                car.Y.OnNext(gameCar.Position.Z);
                car.Orientation.OnNext(gameCar.Orientation + 90);
            }
        }
    }
    /// <summary>
    /// Iterates through _cars and creates Cars on the Radar for missing Ids 
    /// </summary>
    private void CheckForNewCars()
    {
        foreach (var car in _cars.Where(c => _canvasCars.All(x => x.Id != c.Id)))
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

    private void CheckForMissingCars()
    {
        var carsToRemove = _canvasCars.Where(x => _cars.All(c => c.Id != x.Id)).ToArray();
        _carPositions.RemoveAll(x => carsToRemove.Any(c => c.Id == x.Id));
        RadarCanvas.Children.RemoveMany(carsToRemove);
        _canvasCars.RemoveMany(carsToRemove);
    }

    private static double CarWidth => 2 * 10;
    private static double CarLength => 4 * 10;
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        RadarCanvas = this.FindControl<Canvas>("RadarCanvas");
        this.DataContext = new MainWindowViewModel();

        _renderTimer.Start();
    }
    private void MouseDownHandler(object? sender, PointerPressedEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Shift) == 0)
        {
            this.BeginMoveDrag(e);
        }
    }

    private void ScrollHandler(object? sender, PointerWheelEventArgs e)
    {
        if ((e.KeyModifiers & KeyModifiers.Control) != 0)
        {
            ((MainWindowViewModel)DataContext).CanvasScale += (0.1 * e.Delta.Y);
        }

        if ((e.KeyModifiers & KeyModifiers.Shift) != 0)
        {
            ((MainWindowViewModel)DataContext).WindowSize += (10 * e.Delta.X);
        }
    }
}