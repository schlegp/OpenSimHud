using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using rF2SMLib.DTOs;
using rF2SMLib.rFactor2Data;
using rF2SMLib.Utils;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace rF2SMLib
{
    public class TelemetryReceiver
    {
        private const float DEGREES_IN_RADIAN = 57.2957795f;
        private IPEndPoint _endPoint;
        private JsonSerializer serializer;
        private TelemetryProvider _provider;
        public TelemetryReceiver(int port = 11223)
        {
            Client = new UdpClient();
            Client.ExclusiveAddressUse = false;
            _endPoint = new IPEndPoint(IPAddress.Any, port);
            Client.Client.Bind(_endPoint);
            serializer = new JsonSerializer();
            _provider = new TelemetryProvider();
            _provider.Connect().GetAwaiter().GetResult();
        }

        public UdpClient Client { get; }

        public (List<VehicleGPS>, PedalData) GetNextCarTelemetry(double scale = 1.0f)
        {
            
            //
            // var receivedData = await Client.ReceiveAsync();
            //
            // using MemoryStream memoryStream = new  MemoryStream(receivedData.Buffer);
            // using BsonDataReader reader = new BsonDataReader(memoryStream);
            //
            // var wrapper = serializer.Deserialize<VehicleWrapper>(reader);
            var wrapper = _provider.GetListOfAllCars();
            
            var gpsData = wrapper.Vehicles.Select(TelemetryToGps).ToList();
            var player = gpsData.FirstOrDefault(x => x.IsPlayer);
            foreach (var vehicle in gpsData)
            {
                vehicle.DistanceToPlayer = Vector3.Distance(player.Position, vehicle.Position);
            
                vehicle.Position *= (float)scale;
            }
            _ = wrapper;
            _ = player;
            return (gpsData, wrapper.PedalData);
        }

        public VehicleGPS TelemetryToGps(VehicleTelemetryDto tele)
        {
            var id = tele.mID;
            var isPlayer = tele.mIsPlayer == 1;
            var zValue = tele.mOri[2].Z;
            var xValue = tele.mOri[2].X;

            var negativeZVector = new Vector3() {X = 1, Y = 1, Z = -1};

            return new VehicleGPS()
            {
                Id = id,
                Orientation = (Calculation.OriToYaw(zValue, xValue) * DEGREES_IN_RADIAN),
                Position = tele.mPos * negativeZVector,
                IsPlayer = isPlayer,
                DriverName = tele.mDriverName
            };
        }
    }
}