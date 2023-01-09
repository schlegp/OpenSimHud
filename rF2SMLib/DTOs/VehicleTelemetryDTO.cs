using System;
using System.Numerics;
using System.Runtime.Serialization;

namespace rF2SMLib.DTOs
{
    [DataContract]
    public class VehicleTelemetryDto
    {
        [DataMember]
        public int mID { get; set; }
        [DataMember]
        public string mDriverName { get; set; }
        [DataMember]
        public bool IsPlayer { get; set; }
        [DataMember]
        public Vector3 mPos { get; set; }
        [DataMember]
        public Vector3 mLocalAccel { get; set; }
        [DataMember]
        public Vector3[] mOri { get; set; }
        [DataMember]
        public byte mIsPlayer { get; set; }
    }
}