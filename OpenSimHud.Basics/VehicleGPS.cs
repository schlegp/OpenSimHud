using System.Numerics;

namespace OpenSimHud.Basics
{
    public class VehicleGPS
    {
        public int Id { get; set; }
        public string DriverName { get; set; }
        public bool IsPlayer { get; set; }
        public double Orientation { get; set; }
        public Vector3 Position { get; set; }
        public double DistanceToPlayer { get; set; }
    }
}

