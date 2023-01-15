using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using OpenSimHud.Basics;
using OpenSimHud.ViewModels;
using PedalWindow.ViewModels;
using PedalWindow.Views;
using rF2SMLib;
using rF2SMLib.DTOs;
using TireGripWindow.ViewModels;
using TireGripWindow.Views;

namespace OpenSimHud.Views;

public partial class MainWindow : Window
{
    private InputData _currentInputData = new InputData();
    private readonly TelemetryReceiver _receiver;
    
    private readonly PedalDisplayViewModel _pedalDisplayViewModel;
    private readonly PedalDisplay _pedalDisplay;

    private readonly RadarWindow.ViewModels.RadarDisplayViewModel _radarDisplayViewModel;
    private readonly RadarWindow.Views.RadarDisplay _radarDisplay;

    private readonly TireGripViewModel _tireGripViewModel;
    private readonly TireGripDisplay _tireGripDisplay;
    
    private readonly BackgroundWorker _backgroundWorker;
    
    private readonly DispatcherTimer _renderTimer;
    
    /// <summary>
    /// Cars coming from Game
    /// </summary>
    private List<VehicleGPS> _cars;

    public MainWindow()
    {
        _receiver = new TelemetryReceiver();
        _renderTimer = new DispatcherTimer();
        _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
        _renderTimer.Tick += RenderCycle;
        
        _backgroundWorker = new BackgroundWorker();
        _backgroundWorker.DoWork += GetTelemetry;
        _cars = new();
        
        _pedalDisplayViewModel = new PedalDisplayViewModel();
        _pedalDisplay = new PedalDisplay(_pedalDisplayViewModel);
        _pedalDisplay.DataContext = _pedalDisplayViewModel;

        _radarDisplayViewModel = new RadarWindow.ViewModels.RadarDisplayViewModel();
        _radarDisplay = new RadarWindow.Views.RadarDisplay(_radarDisplayViewModel);
        _radarDisplay.DataContext = _radarDisplayViewModel;

        _tireGripViewModel = new TireGripViewModel();
        _tireGripDisplay = new TireGripDisplay(_tireGripViewModel);
        _tireGripDisplay.DataContext = _tireGripDisplay;

        InitializeComponent();

        _pedalDisplay.Show();
        _radarDisplay.Show();
        _tireGripDisplay.Show();
    }
    private void GetTelemetry(object? sender, DoWorkEventArgs e)
    {
        _cars = _receiver.GetNextCarTelemetry().Item1;
        _currentInputData = _receiver.GetNextCarTelemetry().Item2;
    }
    
    private void RenderCycle(object? sender, EventArgs e)
    {
        if (!_backgroundWorker.IsBusy)
        {
            _backgroundWorker.RunWorkerAsync();
        }
        
        // Update Pedals
        _pedalDisplayViewModel.BrakeValue = _currentInputData.BrakePedal * _pedalDisplay.Height;
        _pedalDisplayViewModel.ThrottleValue = _currentInputData.Throttle * _pedalDisplay.Height;

        _radarDisplayViewModel.Cars = _cars;

        _tireGripViewModel.TireGripFl = _currentInputData.GripData[(int)rFactor2Constants.rF2WheelIndex.FrontLeft];
        _tireGripViewModel.TireGripFr = _currentInputData.GripData[(int)rFactor2Constants.rF2WheelIndex.FrontRight];
        _tireGripViewModel.TireGripRl = _currentInputData.GripData[(int)rFactor2Constants.rF2WheelIndex.RearLeft];
        _tireGripViewModel.TireGripRr = _currentInputData.GripData[(int)rFactor2Constants.rF2WheelIndex.RearRight];
    }

    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _renderTimer.Start();
    }
    
    
    

}