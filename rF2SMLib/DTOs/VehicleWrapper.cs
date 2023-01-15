using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace rF2SMLib.DTOs
{
    [DataContract]
    public class VehicleWrapper
    {
        [DataMember]
        public List<VehicleTelemetryDto> Vehicles { get; set; }

        [DataMember] public InputData InputData;
    }

    [DataContract]
    public class InputData
    {
        [DataMember]
        public double Throttle;
        [DataMember]
        public double BrakePedal;
        [DataMember]
        public double Clutch;

        [DataMember] public double SteeringAngle;
        [DataMember] public double[] GripData = new []{0.0, 0.0, 0.0, 0.0 };
    }
}