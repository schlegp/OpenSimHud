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

        [DataMember] public PedalData PedalData;
    }

    [DataContract]
    public class PedalData
    {
        [DataMember]
        public double Throttle;
        [DataMember]
        public double BrakePedal;
        [DataMember]
        public double Clutch;

        [DataMember] public double SteeringAngle;
    }
}