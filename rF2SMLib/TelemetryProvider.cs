using rF2SMLib.rFactor2Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.CodeDom;
using rF2SMLib.DTOs;
using rF2SMLib.Utils;


namespace rF2SMLib
{
    public class TelemetryProvider
    {
        public static MappedBuffer<rF2Telemetry> _telemetryBuffer;
        public static MappedBuffer<rF2Scoring> _scoringBuffer;
        public static rF2Telemetry telemetry;
        public static rF2Scoring scoring;
        public bool AllConnected = false;

        public TelemetryProvider()
        {
            _telemetryBuffer =
                new MappedBuffer<rF2Telemetry>("/dev/shm/" + rFactor2Constants.MM_TELEMETRY_FILE_NAME, true, true);
            _scoringBuffer = new MappedBuffer<rF2Scoring>("/dev/shm/" + rFactor2Constants.MM_SCORING_FILE_NAME, true, true);
        }

        public async Task<bool> Connect()
        {
            await Task.Run(async () =>
            {
                while (_telemetryBuffer?.IsConnected == false)
                {
                    try
                    {
                        _telemetryBuffer.Connect();
                    }
                    catch (Exception e)
                    {
                        await Task.Delay(500);
                    }
                }

                while (_scoringBuffer?.IsConnected == false)
                {
                    try
                    {
                        _scoringBuffer.Connect();
                    }
                    catch
                    {
                        await Task.Delay(500);
                    }
                }
            }).ConfigureAwait(false);
            return true;
        }

        public void GetLatest()
        {
            _telemetryBuffer?.GetMappedDataUnsynchronized(ref telemetry);
            _scoringBuffer.GetMappedDataUnsynchronized(ref scoring);
        }

        public VehicleWrapper GetListOfAllCars()
        {
            GetLatest();
            var carData = telemetry.mVehicles.Where(x =>
                !string.IsNullOrEmpty(Encoding.ASCII.GetString(x.mVehicleName).Replace("\0", ""))).Select(x =>
                new VehicleTelemetryDto()
                {
                    mOri = x.mOri.Select(MathHelper.FromrF2Vec3).ToArray(),
                    mPos = MathHelper.FromrF2Vec3(x.mPos),
                    mID = x.mID,
                    mLocalAccel = MathHelper.FromrF2Vec3(x.mLocalAccel),
                    mDriverName = Encoding.ASCII
                        .GetString(scoring.mVehicles.FirstOrDefault(y => y.mID == x.mID).mDriverName).Replace("\0", ""),
                    mIsPlayer = scoring.mVehicles.FirstOrDefault(y => y.mID == x.mID).mIsPlayer,
                }
            ).ToList();
            var playerId = scoring.mVehicles.FirstOrDefault(x => x.mIsPlayer != 0).mID;
            var playerData = telemetry.mVehicles.FirstOrDefault(x => x.mID == playerId);
            var pedalData = new PedalData()
            {
                Throttle = playerData.mUnfilteredThrottle,
                BrakePedal = playerData.mUnfilteredBrake,
                Clutch = playerData.mUnfilteredClutch,
                SteeringAngle = playerData.mUnfilteredSteering
            };
            var result = new VehicleWrapper()
            {
                Vehicles = carData,
                PedalData = pedalData
            };
            return result;
        }
    }
}