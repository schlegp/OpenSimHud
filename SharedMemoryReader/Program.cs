// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using rF2SMLib;
Console.WriteLine("Hello, World!");
TelemetryProvider telemetryProvider = new TelemetryProvider();
await telemetryProvider.Connect();

var sw = Stopwatch.StartNew();
var cars = telemetryProvider.GetListOfAllCars();

Console.WriteLine(cars.Vehicles.FirstOrDefault()?.mDriverName);
Console.WriteLine(sw.Elapsed);