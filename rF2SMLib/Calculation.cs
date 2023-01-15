using System;
using System.Numerics;
using rF2SMLib.rFactor2Data;

namespace rF2SMLib
{
    public class Calculation
    {
        public static double OriToYaw(double value1, double value2)
        {
            var radian = Math.Atan2(value1, value2);
            return radian;
        }
    }
}

