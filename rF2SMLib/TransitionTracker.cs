/*
TransitionTracker class various state transitions in rF2 state and optionally logs transitions to files.

Author: The Iron Wolf (vleonavicius@hotmail.com)
Website: thecrewchief.org
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using rF2SMLib.rFactor2Data;
using static rF2SMLib.rFactor2Constants;

namespace rF2SMLib
{
  internal class TransitionTracker
  {
    private static readonly string fileTimesTampString = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    private static readonly string basePath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\logs";
    private static readonly string phaseAndStateTrackingFilePath = $"{basePath}\\{fileTimesTampString}___PhaseAndStateTracking.log";
    private static readonly string damageTrackingFilePath = $"{basePath}\\{fileTimesTampString}___DamageTracking.log";
    private static readonly string rulesTrackingFilePath = $"{basePath}\\{fileTimesTampString}___RulesTracking.log";
    private static readonly string phaseAndStateDeltaTrackingFilePath = $"{basePath}\\{fileTimesTampString}___PhaseAndStateTrackingDelta.log";
    private static readonly string damageTrackingDeltaFilePath = $"{basePath}\\{fileTimesTampString}___DamageTrackingDelta.log";
    private static readonly string rulesTrackingDeltaFilePath = $"{basePath}\\{fileTimesTampString}___RulesTrackingDelta.log";
    private static readonly string timingTrackingFilePath = $"{basePath}\\{fileTimesTampString}___TimingTracking.log";

    internal TransitionTracker()
    {
      if (!Directory.Exists(basePath))
        Directory.CreateDirectory(basePath);
    }

    private string GetEnumString<T>(sbyte value)
    {
      var enumType = typeof(T);

      var enumValue = (T)Enum.ToObject(enumType, value);
      return Enum.IsDefined(enumType, enumValue) ? $"{enumValue.ToString()}({value})" : string.Format("Unknown({0})", value);
    }

    private string GetEnumString<T>(byte value)
    {
      var enumType = typeof(T);

      var enumValue = (T)Enum.ToObject(enumType, value);
      return Enum.IsDefined(enumType, enumValue) ? $"{enumValue.ToString()}({value})" : string.Format("Unknown({0})", value);
    }

    public static string GetSessionString(int session)
    {
      // current session (0=testday 1-4=practice 5-8=qual 9=warmup 10-13=race)
      if (session == 0)
        return $"TestDay({session})";
      else if (session >= 1 && session <= 4)
        return $"Practice({session})";
      else if (session >= 5 && session <= 8)
        return $"Qualification({session})";
      else if (session == 9)
        return $"WarmUp({session})";
      else if (session >= 10 && session <= 13)
        return $"Race({session})";

      return $"Unknown({session})";
    }


    // Telemetry values (separate section)

    internal class PhaseAndState
    {
      internal rF2GamePhase mGamePhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);
      internal int mSession = -255;
      internal rF2YellowFlagState mYellowFlagState = (rF2YellowFlagState)Enum.ToObject(typeof(rF2YellowFlagState), -255);
      internal int mSector = -255;
      internal int mCurrentSector = -255;
      internal byte mInRealTimeFC = 255;
      internal byte mInRealTime = 255;
      internal rF2YellowFlagState mSector1Flag = (rF2YellowFlagState)Enum.ToObject(typeof(rF2YellowFlagState), -255);
      internal rF2YellowFlagState mSector2Flag = (rF2YellowFlagState)Enum.ToObject(typeof(rF2YellowFlagState), -255);
      internal rF2YellowFlagState mSector3Flag = (rF2YellowFlagState)Enum.ToObject(typeof(rF2YellowFlagState), -255);
      internal rF2Control mControl;
      internal byte mInPits = 255;
      internal byte mIsPlayer = 255;
      internal int mPlace = -255;
      internal rF2PitState mPitState = (rF2PitState)Enum.ToObject(typeof(rF2PitState), -255);
      internal rF2GamePhase mIndividualPhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);
      internal rF2PrimaryFlag mFlag = (rF2PrimaryFlag)Enum.ToObject(typeof(rF2PrimaryFlag), -255);
      internal byte mUnderYellow = 255;
      internal rF2CountLapFlag mCountLapFlag = (rF2CountLapFlag)Enum.ToObject(typeof(rF2CountLapFlag), -255);
      internal byte mInGarageStall = 255;
      internal rF2FinishStatus mFinishStatus = (rF2FinishStatus)Enum.ToObject(typeof(rF2FinishStatus), -255);
      internal int mLapNumber = -255;
      internal short mTotalLaps = -255;
      internal int mMaxLaps = -1;
      internal int mNumVehicles = -1;
      internal byte mScheduledStops = 255;
      internal byte mHeadlights = 255;
      internal byte mSpeedLimiter = 255;
      internal byte mFrontTireCompoundIndex = 255;
      internal byte mRearTireCompoundIndex = 255;
      internal string mFrontTireCompoundName = "Unknown";
      internal string mRearTireCompoundName = "Unknown";
      internal byte mFrontFlapActivated = 255;
      internal byte mRearFlapActivated = 255;
      internal rF2RearFlapLegalStatus mRearFlapLegalStatus = (rF2RearFlapLegalStatus)Enum.ToObject(typeof(rF2RearFlapLegalStatus), -255);
      internal rF2IgnitionStarterStatus mIgnitionStarter = (rF2IgnitionStarterStatus)Enum.ToObject(typeof(rF2IgnitionStarterStatus), -255);
      internal byte mSpeedLimiterAvailable = 255;
      internal byte mAntiStallActivated = 255;
      internal byte mStartLight = 255;
      internal byte mNumRedLights = 255;
      internal short mNumPitstops = -255;
      internal short mNumPenalties = -255;
      internal int mLapsBehindNext = -1;
      internal int mLapsBehindLeader = -1;
      internal byte mPlayerHeadlights = 255;
      internal byte mServerScored = 255;
      internal int mQualification = -1;
    }

    internal PhaseAndState prevPhaseAndSate = new PhaseAndState();
    internal StringBuilder sbPhaseChanged = new StringBuilder();
    internal StringBuilder sbPhaseLabel = new StringBuilder();
    internal StringBuilder sbPhaseValues = new StringBuilder();
    internal StringBuilder sbPhaseChangedCol2 = new StringBuilder();
    internal StringBuilder sbPhaseLabelCol2 = new StringBuilder();
    internal StringBuilder sbPhaseValuesCol2 = new StringBuilder();

    rF2GamePhase lastDamageTrackingGamePhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);
    rF2GamePhase lastPhaseTrackingGamePhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);
    rF2GamePhase lastTimingTrackingGamePhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);
    rF2GamePhase lastRulesTrackingGamePhase = (rF2GamePhase)Enum.ToObject(typeof(rF2GamePhase), -255);

    private float screenYStart = 253.0f;

    private static string GetStringFromBytes(byte[] bytes)
    {
      if (bytes == null)
        return "";

      var nullIdx = Array.IndexOf(bytes, (byte)0);

      return nullIdx >= 0
        ? Encoding.Default.GetString(bytes, 0, nullIdx)
        : Encoding.Default.GetString(bytes);
    }

    internal class DamageInfo
    {
      internal byte[] mDentSeverity = new byte[8];         // dent severity at 8 locations around the car (0=none, 1=some, 2=more)
      internal double mLastImpactMagnitude = -1.0;   // magnitude of last impact
      internal double mAccumulatedImpactMagnitude = -1.0;   // magnitude of last impact
      internal double mMaxImpactMagnitude = -1.0;   // magnitude of last impact
      internal rF2Vec3 mLastImpactPos;        // location of last impact
      internal double mLastImpactET = -1.0;          // time of last impact
      internal byte mOverheating = 255;            // whether overheating icon is shown
      internal byte mDetached = 255;               // whether any parts (besides wheels) have been detached
      //internal byte mHeadlights = 255;             // whether headlights are on

      internal byte mFrontLeftFlat = 255;                    // whether tire is flat
      internal byte mFrontLeftDetached = 255;                // whether wheel is detached
      internal byte mFrontRightFlat = 255;                    // whether tire is flat
      internal byte mFrontRightDetached = 255;                // whether wheel is detached

      internal byte mRearLeftFlat = 255;                    // whether tire is flat
      internal byte mRearLeftDetached = 255;                // whether wheel is detached
      internal byte mRearRightFlat = 255;                    // whether tire is flat
      internal byte mRearRightDetached = 255;                // whether wheel is detached
    }

    internal DamageInfo prevDamageInfo = new DamageInfo();
    internal StringBuilder sbDamageChanged = new StringBuilder();
    internal StringBuilder sbDamageLabel = new StringBuilder();
    internal StringBuilder sbDamageValues = new StringBuilder();

    internal class PlayerTimingInfo
    {
      internal string name = null;
      internal double lastS1Time = -1.0;
      internal double lastS2Time = -1.0;
      internal double lastS3Time = -1.0;

      internal double currS1Time = -1.0;
      internal double currS2Time = -1.0;
      internal double currS3Time = -1.0;

      internal double bestS1Time = -1.0;
      internal double bestS2Time = -1.0;
      internal double bestS3Time = -1.0;

      internal double currLapET = -1.0;
      internal double lastLapTime = -1.0;
      internal double currLapTime = -1.0;
      internal double bestLapTime = -1.0;

      internal int currLap = -1;
    }

    internal class OpponentTimingInfo
    {
      internal string name = null;
      internal int position = -1;
      internal double lastS1Time = -1.0;
      internal double lastS2Time = -1.0;
      internal double lastS3Time = -1.0;

      internal double currS1Time = -1.0;
      internal double currS2Time = -1.0;
      internal double currS3Time = -1.0;

      internal double bestS1Time = -1.0;
      internal double bestS2Time = -1.0;
      internal double bestS3Time = -1.0;

      internal double currLapET = -1.0;
      internal double lastLapTime = -1.0;
      internal double currLapTime = -1.0;
      internal double bestLapTime = -1.0;

      internal int currLap = -1;

      internal string vehicleName = null;
      internal string vehicleClass = null;

      internal long mID = -1;
    }

    // string -> lap data

    internal class LapData
    {
      internal class LapStats
      {
        internal int lapNumber = -1;
        internal double lapTime = -1.0;
        internal double S1Time = -1.0;
        internal double S2Time = -1.0;
        internal double S3Time = -1.0;
      }

      internal int lastLapCompleted = -1;
      internal List<LapStats> lapStats = new List<LapStats>();
    }

    internal Dictionary<string, LapData> lapDataMap = null;

    int lastTimingSector = -1;
    string bestSplitString = "";

    private int GetSector(int rf2Sector) { return rf2Sector == 0 ? 3 : rf2Sector; }
    private string LapTimeStr(double time)
    {
      return time > 0.0 ? TimeSpan.FromSeconds(time).ToString(@"mm\:ss\:fff") : time.ToString();
    }

    private LapData.LapStats getBestLapStats(string opponentName, bool skipLastLap)
    {
      LapData.LapStats bestLapStats = new LapData.LapStats();
      if (this.lapDataMap.ContainsKey(opponentName))
      {
        var opLd = this.lapDataMap[opponentName];

        double bestLapTimeTracked = -1.0;
        var lapsToCheck = opLd.lapStats.Count;
        if (skipLastLap)
          --lapsToCheck;

        for (int i = 0; i < lapsToCheck; ++i)
        {
          var ls = opLd.lapStats[i];
          if (bestLapStats.lapTime < 0.0
            || ls.lapTime < bestLapTimeTracked)
          {
            bestLapTimeTracked = ls.lapTime;
            bestLapStats = ls;
          }
        }
      }

      return bestLapStats;
    }

    internal class Rules
    {
      public rF2TrackRulesStage mStage = (rF2TrackRulesStage)Enum.ToObject(typeof(rF2TrackRulesStage), -255);
      public rF2TrackRulesColumn mPoleColumn = (rF2TrackRulesColumn)Enum.ToObject(typeof(rF2TrackRulesColumn), -255);      // column assignment where pole position seems to be located
      public int mNumActions = -1;                     // number of recent actions
      public int mNumParticipants = -1;                // number of participants (vehicles)

      public byte mYellowFlagDetected = 255;             // whether yellow flag was requested or sum of participant mYellowSeverity's exceeds mSafetyCarThreshold
      public byte mYellowFlagLapsWasOverridden = 255;    // whether mYellowFlagLaps (below) is an admin request

      public byte mSafetyCarExists = 255;                // whether safety car even exists
      public byte mSafetyCarActive = 255;                // whether safety car is active
      public int mSafetyCarLaps = 255;                  // number of laps
      public float mSafetyCarThreshold = -1.0f;            // the threshold at which a safety car is called out (compared to the sum of TrackRulesParticipantV01::mYellowSeverity for each vehicle)
      public double mSafetyCarLapDist;             // safety car lap distance
      public float mSafetyCarLapDistAtStart;       // where the safety car starts from

      public float mPitLaneStartDist = -1.0f;              // where the waypoint branch to the pits breaks off (this may not be perfectly accurate)
      public float mTeleportLapDist = -1.0f;               // the front of the teleport locations (a useful first guess as to where to throw the green flag)

      // input/output
      public sbyte mYellowFlagState = 127;         // see ScoringInfoV01 for values
      public short mYellowFlagLaps = 127;                // suggested number of laps to run under yellow (may be passed in with admin command)

      public rF2SafetyCarInstruction mSafetyCarInstruction = (rF2SafetyCarInstruction)Enum.ToObject(typeof(rF2SafetyCarInstruction), -255);
      public float mSafetyCarSpeed = -1.0f;                // maximum speed at which to drive
      public float mSafetyCarMinimumSpacing = -2.0f;       // minimum spacing behind safety car (-1 to indicate no limit)
      public float mSafetyCarMaximumSpacing = -2.0f;       // maximum spacing behind safety car (-1 to indicate no limit)

      public float mMinimumColumnSpacing = -2.0f;          // minimum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)
      public float mMaximumColumnSpacing = -2.0f;          // maximum desired spacing between vehicles in a column (-1 to indicate indeterminate/unenforced)

      public float mMinimumSpeed = -2.0f;                  // minimum speed that anybody should be driving (-1 to indicate no limit)
      public float mMaximumSpeed = -2.0f;                  // maximum speed that anybody should be driving (-1 to indicate no limit)

      public string mMessage = "unknown";                  // a message for everybody to explain what is going on (which will get run through translator on client machines)

      public short mFrozenOrder = 127;                           // 0-based place when caution came out (not valid for formation laps)
      public short mPlace = 127;                                 // 1-based place (typically used for the initialization of the formation lap track order)
      public float mYellowSeverity = -1.0f;                        // a rating of how much this vehicle is contributing to a yellow flag (the sum of all vehicles is compared to TrackRulesV01::mSafetyCarThreshold)
      public double mCurrentRelativeDistance = -1.0;              // equal to ( ( ScoringInfoV01::mLapDist * this->mRelativeLaps ) + VehicleScoringInfoV01::mLapDist )

      // input/output
      public int mRelativeLaps = -1;                            // current formation/caution laps relative to safety car (should generally be zero except when safety car crosses s/f line); this can be decremented to implement 'wave around' or 'beneficiary rule' (a.k.a. 'lucky dog' or 'free pass')
      public rF2TrackRulesColumn mColumnAssignment = (rF2TrackRulesColumn)Enum.ToObject(typeof(rF2TrackRulesColumn), -255);        // which column (line/lane) that participant is supposed to be in
      public int mPositionAssignment = -1;                      // 0-based position within column (line/lane) that participant is supposed to be located at (-1 is invalid)
      public byte mPitsOpen = 255;                           // whether the rules allow this particular vehicle to enter pits right now

      public double mGoalRelativeDistance = -1.0;                 // calculated based on where the leader is, and adjusted by the desired column spacing and the column/position assignments

      public string mMessage_Participant = "unknown";                  // a message for this participant to explain what is going on (untranslated; it will get run through translator on client machines)
    }

    internal Rules prevRules = new Rules();
    internal StringBuilder sbRulesChanged = new StringBuilder();
    internal StringBuilder sbRulesLabel = new StringBuilder();
    internal StringBuilder sbRulesValues = new StringBuilder();
    internal StringBuilder sbFrozenOrderInfo = new StringBuilder();
    internal StringBuilder sbFrozenOrderOnlineInfo = new StringBuilder();
    private FrozenOrderData prevFrozenOrderData;
    private FrozenOrderData prevFrozenOrderDataOnline;
    private long ticksLSIOrderInstructionMessageUpdated = 0L;
    private int numFODetectPhaseAttempts = 0;

    public enum FrozenOrderPhase
    {
      None,
      FullCourseYellow,
      FormationStanding,
      Rolling,
      FastRolling
    }

    public enum FrozenOrderColumn
    {
      None,
      Left,
      Right
    }

    public enum FrozenOrderAction
    {
      None,
      Follow,
      CatchUp,
      AllowToPass,
      StayInPole,  // Case of being assigned pole/pole row with no SC present (Rolling start in rF2 Karts, for example).
      MoveToPole  // Case of falling behind assigned pole/pole row with no SC present (Rolling start in rF2 Karts, for example).
    }

    public class FrozenOrderData
    {
      public FrozenOrderPhase Phase = FrozenOrderPhase.None;
      public FrozenOrderAction Action = FrozenOrderAction.None;

      // If column is assigned, p1 and p2 follows SC.  Otherwise,
      // only p1 follows SC.
      public int AssignedPosition = -1;

      public FrozenOrderColumn AssignedColumn = FrozenOrderColumn.None;
      // Only matters if AssignedColumn != None
      public int AssignedGridPosition = -1;

      public string DriverToFollow = "";

      // Meters/s.  If -1, SC either left or not present.
      public float SafetyCarSpeed = -1.0f;
    }

    private FrozenOrderData GetFrozenOrderData(FrozenOrderData prevFrozenOrderData, ref rF2VehicleScoring vehicle,
      ref rF2Scoring scoring, ref rF2TrackRulesParticipant vehicleRules, ref rF2Rules rules, ref rF2Extended extended, ref double distToSC)
    {
      var fod = new FrozenOrderData();

      // Only applies to formation laps and FCY.
      if (scoring.mScoringInfo.mGamePhase != (int)rF2GamePhase.Formation
        && scoring.mScoringInfo.mGamePhase != (int)rF2GamePhase.FullCourseYellow)
      {
        this.numFODetectPhaseAttempts = 0;
        return fod;
      }

      var foStage = rules.mTrackRules.mStage;
      if (foStage == rF2TrackRulesStage.Normal)
        return fod; // Note, there's slight race between scoring and rules here, FO messages should have validation on them.

      if (extended.mDirectMemoryAccessEnabled != 0)
      {
        if (prevFrozenOrderData == null || prevFrozenOrderData.Phase == FrozenOrderPhase.None)
        {
          // Don't bother checking updated ticks, this showld allow catching multiple SC car phases.
          var phase = TransitionTracker.GetStringFromBytes(extended.mLSIPhaseMessage);

          if (scoring.mScoringInfo.mGamePhase == (int)rF2GamePhase.Formation
            && string.IsNullOrWhiteSpace(phase))
          {
            if (this.numFODetectPhaseAttempts > 0)
              fod.Phase = FrozenOrderPhase.FormationStanding;

            ++this.numFODetectPhaseAttempts;
          }
          else if (!string.IsNullOrWhiteSpace(phase)
            && phase == "Formation Lap")
          {
            var speed = Math.Sqrt((vehicle.mLocalVel.x * vehicle.mLocalVel.x)
              + (vehicle.mLocalVel.y * vehicle.mLocalVel.y)
              + (vehicle.mLocalVel.z * vehicle.mLocalVel.z));

            fod.Phase = this.GetSector(vehicle.mSector) == 3 && speed > 10.0 ? FrozenOrderPhase.FastRolling : FrozenOrderPhase.Rolling;
          }
          else if (!string.IsNullOrWhiteSpace(phase)
            && (phase == "Full-Course Yellow" || phase == "One Lap To Go"))
            fod.Phase = FrozenOrderPhase.FullCourseYellow;
          else if (string.IsNullOrWhiteSpace(phase))
            fod.Phase = prevFrozenOrderData.Phase;
        }
        else
          fod.Phase = prevFrozenOrderData.Phase;
      }
      else
      {
        // rF2 currently does not expose what kind of race start is chosen.  For tracks with SC, I use presence of SC to distinguish between
        // Formation/Standing and Rolling starts.  However, if SC does not exist (Kart tracks), I used the fact that in Rolling start leader is
        // typically standing past S/F line (mLapDist is positive).  Obviously, there will be perverted tracks where that won't be true, but this
        // all I could come up with, and real problem is in game being shit in this area.
        var leaderLapDistAtFOPhaseStart = 0.0;
        var leaderSectorAtFOPhaseStart = -1;
        if (foStage != rF2TrackRulesStage.CautionInit && foStage != rF2TrackRulesStage.CautionUpdate  // If this is not FCY.
          && (prevFrozenOrderData == null || prevFrozenOrderData.Phase == FrozenOrderPhase.None)  // And, this is first FO calculation.
          && rules.mTrackRules.mSafetyCarExists == 0) // And, track has no SC.
        {
          // Find where leader is relatively to F/S line.
          for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
          {
            var veh = scoring.mVehicles[i];
            if (veh.mPlace == 1)
            {
              leaderLapDistAtFOPhaseStart = veh.mLapDist;
              leaderSectorAtFOPhaseStart = this.GetSector(veh.mSector);
              break;
            }
          }
        }

        // Figure out the phase:
        if (foStage == rF2TrackRulesStage.CautionInit || foStage == rF2TrackRulesStage.CautionUpdate)
          fod.Phase = FrozenOrderPhase.FullCourseYellow;
        else if (foStage == rF2TrackRulesStage.FormationInit || foStage == rF2TrackRulesStage.FormationUpdate)
        {
          // Check for signs of a rolling start.
          if ((prevFrozenOrderData != null && prevFrozenOrderData.Phase == FrozenOrderPhase.Rolling)  // If FO started as Rolling, keep it as Rolling even after SC leaves the track
            || (rules.mTrackRules.mSafetyCarExists == 1 && rules.mTrackRules.mSafetyCarActive == 1)  // Of, if SC exists and is active
            || (rules.mTrackRules.mSafetyCarExists == 0 && leaderLapDistAtFOPhaseStart > 0.0 && leaderSectorAtFOPhaseStart == 1)) // Or, if SC is not present on a track, and leader started ahead of S/F line and is insector 1.  This will be problem on some tracks.
            fod.Phase = FrozenOrderPhase.Rolling;
          else
          {
            // Formation / Standing and Fast Rolling have no Safety Car.
            fod.Phase = rules.mTrackRules.mStage == rF2TrackRulesStage.FormationInit && this.GetSector(vehicle.mSector) == 3
              ? FrozenOrderPhase.FastRolling  // Fast rolling never goes into FormationUpdate and usually starts in S3.
              : FrozenOrderPhase.FormationStanding;
          }
        }
      }

      if (fod.Phase == FrozenOrderPhase.None)
        return fod;  // Wait a bit, there's a delay for string based phases.

      if (vehicleRules.mPositionAssignment != -1)
      {
        var gridOrder = false;

        var scrLastLapDoubleFile = fod.Phase == FrozenOrderPhase.FullCourseYellow
          && extended.mSCRPluginEnabled == 1
          && (extended.mSCRPluginDoubleFileType == 1 || extended.mSCRPluginDoubleFileType == 2)
          && scoring.mScoringInfo.mYellowFlagState == (sbyte)rF2YellowFlagState.LastLap;

        if (fod.Phase == FrozenOrderPhase.FullCourseYellow  // Core FCY does not use grid order. 
          && !scrLastLapDoubleFile)  // With SCR rules, however, last lap might be double file depending on DoubleFileType configuration var value.
        {
          gridOrder = false;
          fod.AssignedPosition = vehicleRules.mPositionAssignment + 1;  // + 1, because it is zero based with 0 meaning follow SC.
        }
        else  // This is not FCY, or last lap of Double File FCY with SCR plugin enabled.  The order reported is grid order, with columns specified.
        {
          gridOrder = true;
          fod.AssignedGridPosition = vehicleRules.mPositionAssignment + 1;
          fod.AssignedColumn = vehicleRules.mColumnAssignment == rF2TrackRulesColumn.LeftLane ? FrozenOrderColumn.Left : FrozenOrderColumn.Right;

          if (rules.mTrackRules.mPoleColumn == rF2TrackRulesColumn.LeftLane)
          {
            fod.AssignedPosition = (vehicleRules.mColumnAssignment == rF2TrackRulesColumn.LeftLane
              ? vehicleRules.mPositionAssignment * 2
              : vehicleRules.mPositionAssignment * 2 + 1) + 1;
          }
          else if (rules.mTrackRules.mPoleColumn == rF2TrackRulesColumn.RightLane)
          {
            fod.AssignedPosition = (vehicleRules.mColumnAssignment == rF2TrackRulesColumn.RightLane
              ? vehicleRules.mPositionAssignment * 2
              : vehicleRules.mPositionAssignment * 2 + 1) + 1;
          }

        }

        // Figure out Driver Name to follow.
        // NOTE: In Formation/Standing, game does not report those in UI, but we could.
        var vehToFollowId = -1;
        var followSC = true;
        if ((gridOrder && fod.AssignedPosition > 2)  // In grid order, first 2 vehicles are following SC.
          || (!gridOrder && fod.AssignedPosition > 1))  // In non-grid order, 1st car is following SC.
        {
          followSC = false;
          // Find the mID of a vehicle in front of us by frozen order.
          for (int i = 0; i < rules.mTrackRules.mNumParticipants; ++i)
          {
            var p = rules.mParticipants[i];
            if ((!gridOrder  // Don't care about column in non-grid order case.
                || (gridOrder && p.mColumnAssignment == vehicleRules.mColumnAssignment))  // Should be vehicle in the same column.
              && p.mPositionAssignment == (vehicleRules.mPositionAssignment - 1))
            {
              vehToFollowId = p.mID;
              break;
            }
          }
        }

        var playerDist = this.GetDistanceCompleteded(ref scoring, ref vehicle);
        var toFollowDist = -1.0;

        if (!followSC)
        {
          // Now find the vehicle to follow from the scoring info.
          for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
          {
            var v = scoring.mVehicles[i];
            if (v.mID == vehToFollowId)
            {
              fod.DriverToFollow = TransitionTracker.GetStringFromBytes(v.mDriverName);

              toFollowDist = this.GetDistanceCompleteded(ref scoring, ref v);
              break;
            }
          }
        }
        else
          toFollowDist = ((vehicle.mTotalLaps - vehicleRules.mRelativeLaps) * scoring.mScoringInfo.mLapDist) + rules.mTrackRules.mSafetyCarLapDist;

        distToSC = rules.mTrackRules.mSafetyCarActive == 1
          ? (((vehicle.mTotalLaps - vehicleRules.mRelativeLaps) * scoring.mScoringInfo.mLapDist) + rules.mTrackRules.mSafetyCarLapDist) - playerDist
          : -1.0;

        if (fod.Phase == FrozenOrderPhase.Rolling
          && followSC 
          && rules.mTrackRules.mSafetyCarExists == 0)
        {
          // Find distance to car next to us if we're in pole.
          var neighborDist = -1.0;
          for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
          {
            var veh = scoring.mVehicles[i];
            if (veh.mPlace == (vehicle.mPlace == 1 ? 2 : 1))
            {
              neighborDist = this.GetDistanceCompleteded(ref scoring, ref veh);
              break;
            }
          }

          var distDelta = neighborDist - playerDist;
          // Special case if we have to stay in pole row, but there's no SC on this track.
          if (fod.AssignedColumn == FrozenOrderColumn.None)
            fod.Action = distDelta > 70.0 ? FrozenOrderAction.MoveToPole : FrozenOrderAction.StayInPole;
          else
            fod.Action = distDelta > 70.0 ? FrozenOrderAction.MoveToPole : FrozenOrderAction.StayInPole;
        }
        else
        {
          Debug.Assert(toFollowDist != -1.0);

          fod.Action = FrozenOrderAction.Follow;

          var distDelta = toFollowDist - playerDist;
          if (distDelta < 0.0)
            fod.Action = FrozenOrderAction.AllowToPass;
          else if (distDelta > 70.0)
            fod.Action = FrozenOrderAction.CatchUp;
        }
      }

      if (rules.mTrackRules.mSafetyCarActive == 1)
        fod.SafetyCarSpeed = rules.mTrackRules.mSafetyCarSpeed;

      return fod;
    }

    private FrozenOrderData GetFrozenOrderOnlineData(FrozenOrderData prevFrozenOrderData, ref rF2VehicleScoring vehicle,
      ref rF2Scoring scoring, ref rF2Extended extended, ref double distToSC)
    {
      if (extended.mDirectMemoryAccessEnabled == 0)
        return null;

      var fod = new FrozenOrderData();

      // Only applies to formation laps and FCY.
      if (scoring.mScoringInfo.mGamePhase != (int)rF2GamePhase.Formation
        && scoring.mScoringInfo.mGamePhase != (int)rF2GamePhase.FullCourseYellow)
      {
        this.numFODetectPhaseAttempts = 0;
        return fod;
      }

      if (prevFrozenOrderData != null)
      {
        // Carry old state over.
        fod.Action = prevFrozenOrderData.Action;
        fod.AssignedColumn = prevFrozenOrderData.AssignedColumn;
        fod.AssignedPosition = prevFrozenOrderData.AssignedPosition;
        fod.AssignedGridPosition = prevFrozenOrderData.AssignedGridPosition;
        fod.DriverToFollow = prevFrozenOrderData.DriverToFollow;
        fod.Phase = prevFrozenOrderData.Phase;
        fod.SafetyCarSpeed = prevFrozenOrderData.SafetyCarSpeed;
      }

      if (fod.Phase == FrozenOrderPhase.None)
      {
        // Don't bother checking updated ticks, this showld allow catching multiple SC car phases.
        var phase = TransitionTracker.GetStringFromBytes(extended.mLSIPhaseMessage);

        if (scoring.mScoringInfo.mGamePhase == (int)rF2GamePhase.Formation
          && string.IsNullOrWhiteSpace(phase))
        {
          if (this.numFODetectPhaseAttempts > 0)
            fod.Phase = FrozenOrderPhase.FormationStanding;

          ++this.numFODetectPhaseAttempts;
        }
        else if (!string.IsNullOrWhiteSpace(phase)
          && phase == "Formation Lap")
        {
          var speed = Math.Sqrt((vehicle.mLocalVel.x * vehicle.mLocalVel.x)
            + (vehicle.mLocalVel.y * vehicle.mLocalVel.y)
            + (vehicle.mLocalVel.z * vehicle.mLocalVel.z));

          fod.Phase = this.GetSector(vehicle.mSector) == 3 && speed > 10.0 ? FrozenOrderPhase.FastRolling : FrozenOrderPhase.Rolling;
        }
        else if (!string.IsNullOrWhiteSpace(phase)
          && (phase == "Full-Course Yellow" || phase == "One Lap To Go"))
          fod.Phase = FrozenOrderPhase.FullCourseYellow;
        else if (string.IsNullOrWhiteSpace(phase))
          fod.Phase = prevFrozenOrderData.Phase;
      }

      if (fod.Phase == FrozenOrderPhase.None)
        return fod;  // Wait a bit, there's a delay for string based phases.

      // NOTE: for formation/standing capture order once.   For other phases, rely on LSI text.
      if ((fod.Phase == FrozenOrderPhase.FastRolling || fod.Phase == FrozenOrderPhase.Rolling || fod.Phase == FrozenOrderPhase.FullCourseYellow)
        && this.ticksLSIOrderInstructionMessageUpdated != extended.mTicksLSIOrderInstructionMessageUpdated)
      {
        this.ticksLSIOrderInstructionMessageUpdated = extended.mTicksLSIOrderInstructionMessageUpdated;

        var orderInstruction = TransitionTracker.GetStringFromBytes(extended.mLSIOrderInstructionMessage);
        if (!string.IsNullOrWhiteSpace(orderInstruction))
        {
          var followPrefix = @"Please Follow ";
          var catchUpToPrefix = @"Please Catch Up To ";
          var allowToPassPrefix = @"Please Allow ";
          
          var action = FrozenOrderAction.None;

          string prefix = null;
          if (orderInstruction.StartsWith(followPrefix))
          {
            prefix = followPrefix;
            action = FrozenOrderAction.Follow;
          }
          else if (orderInstruction.StartsWith(catchUpToPrefix))
          {
            prefix = catchUpToPrefix;
            action = FrozenOrderAction.CatchUp;
          }
          else if (orderInstruction.StartsWith(allowToPassPrefix))
          {
            prefix = allowToPassPrefix;
            action = FrozenOrderAction.AllowToPass;
          }
          else
            Debug.Assert(false, "unhandled action");

          if (!string.IsNullOrWhiteSpace(prefix))
          {
            var closingQuoteIdx = orderInstruction.LastIndexOf("\"");
            string driverName = null;
            try
            {
              if (closingQuoteIdx != -1)
              {
                driverName = orderInstruction.Substring(prefix.Length + 1, closingQuoteIdx - prefix.Length - 1);
              }
              else
              {
                driverName = "Safety Car";
              }
            }
            catch (Exception){}

            // Remove [-0.2 laps] if it is there.
            var lastOpenBckt = orderInstruction.LastIndexOf('[');
            if (lastOpenBckt != -1)
            {
              try
              {
                orderInstruction = orderInstruction.Substring(0, lastOpenBckt - 1);
              }
              catch (Exception) {}
            }

            var column = FrozenOrderColumn.None;
            if (orderInstruction.EndsWith("(In Right Line)"))
              column = FrozenOrderColumn.Right;
            else if (orderInstruction.EndsWith("(In Left Line)"))
              column = FrozenOrderColumn.Left;
            else if (!orderInstruction.EndsWith("\"") && action != FrozenOrderAction.AllowToPass)
              Debug.Assert(false, "unrecognized postfix");

            // Note: assigned Grid position only matters for Formation/Standing - don't bother figuring it out, just figure out assigned position (starting position).
            var assignedPos = -1;
            if (!string.IsNullOrWhiteSpace(driverName))
            {
              if (driverName != "Safety Car")
              {
                for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
                {
                  var veh = scoring.mVehicles[i];
                  var driver = TransitionTracker.GetStringFromBytes(veh.mDriverName);
                  if (driver == driverName)
                  {
                    if (column == FrozenOrderColumn.None)
                    {
                      assignedPos = action == FrozenOrderAction.Follow || action == FrozenOrderAction.CatchUp
                        ? veh.mPlace + 1
                        : veh.mPlace - 1; // Might not be true
                    }
                    else
                    {
                      assignedPos = action == FrozenOrderAction.Follow || action == FrozenOrderAction.CatchUp
                        ? veh.mPlace + 2
                        : veh.mPlace - 2; // Might not be true
                    }
                    break;
                  }
                }
              }
              else
              {
                assignedPos = vehicle.mPlace;
              }
            }

            fod.Action = action;
            fod.AssignedColumn = column;
            fod.DriverToFollow = driverName;
            fod.AssignedPosition = assignedPos;
          }
        }
      }
      else if ((prevFrozenOrderData == null || prevFrozenOrderData.Phase == FrozenOrderPhase.None)
        && fod.Phase == FrozenOrderPhase.FormationStanding)
      {
        // Just capture the starting position.
        fod.AssignedColumn = vehicle.mTrackEdge > 0.0 ? FrozenOrderColumn.Right : FrozenOrderColumn.Left;
        fod.AssignedPosition = vehicle.mPlace;

        // We need to know which side of a grid leader is here, gosh what a bullshit.
        // Find where leader is relatively to F/S line.
        var leaderCol = FrozenOrderColumn.None;
        for (int i = 0; i < scoring.mScoringInfo.mNumVehicles; ++i)
        {
          var veh = scoring.mVehicles[i];
          if (veh.mPlace == 1)
          {
            leaderCol = veh.mTrackEdge > 0.0 ? FrozenOrderColumn.Right : FrozenOrderColumn.Left;
            break;
          }
        }

        if (fod.AssignedColumn == FrozenOrderColumn.Left)
        {
          fod.AssignedGridPosition = leaderCol == FrozenOrderColumn.Left 
            ? (vehicle.mPlace / 2) + 1 
            : vehicle.mPlace / 2;
        }
        else if (fod.AssignedColumn == FrozenOrderColumn.Right)
        {
          fod.AssignedGridPosition = leaderCol == FrozenOrderColumn.Right
            ? (vehicle.mPlace / 2) + 1
            : vehicle.mPlace / 2;
        }
      }

      return fod;
    }

    private double GetDistanceCompleteded(ref rF2Scoring scoring, ref rF2VehicleScoring vehicle)
    {
      // Note: Can be interpolated a bit.
      return vehicle.mTotalLaps * scoring.mScoringInfo.mLapDist + vehicle.mLapDist;
    }
  }
}
