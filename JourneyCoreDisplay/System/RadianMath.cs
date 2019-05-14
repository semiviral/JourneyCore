using System;

namespace JourneyCoreLib.System
{
    public static class RadianMath
    {
        public static float CosFromDegrees(float degrees)
        {
            return (float)Math.Cos(degrees * (Math.PI / 180));
        }

        public static float SinFromDegrees(float degrees)
        {
            return (float)Math.Sin(degrees * (Math.PI / 180));
        }
    }
}
