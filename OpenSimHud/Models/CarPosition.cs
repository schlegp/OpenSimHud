using System.Reactive.Subjects;

namespace OpenSimHud.Models;

public class CarPosition
{
    public int Id { get; set; }
    public readonly BehaviorSubject<double> X = new (0);
    public readonly BehaviorSubject<double> Y = new (0);
    public readonly BehaviorSubject<double> Distance = new (0);
    public readonly BehaviorSubject<double> Orientation = new(0);
}