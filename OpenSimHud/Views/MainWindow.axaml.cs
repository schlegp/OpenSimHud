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

namespace OpenSimHud.Views;

public partial class MainWindow : Window
{
    private PedalData _currentPedalData = new PedalData();
    private readonly TelemetryReceiver _receiver;
    
    private readonly PedalDisplayViewModel _pedalDisplayViewModel;
    private readonly PedalDisplay _pedalDisplay;

    private readonly RadarWindow.ViewModels.RadarDisplayViewModel _radarDisplayViewModel;
    private readonly RadarWindow.Views.RadarDisplay _radarDisplay;
    
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

        InitializeComponent();

        _pedalDisplay.Show();
        _radarDisplay.Show();
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
        
        // Update Pedals
        _pedalDisplayViewModel.BrakeValue = _currentPedalData.BrakePedal * _pedalDisplay.Height;
        _pedalDisplayViewModel.ThrottleValue = _currentPedalData.Throttle * _pedalDisplay.Height;

        _radarDisplayViewModel.Cars = _cars;
    }

    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        _renderTimer.Start();
    }
    
    
    

}