using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;
using SFML.Graphics;

namespace JourneyCore.Lib.System
{
    public static class ValueTypeExtensionMethods
    {
        public static float LimitToRange(this float subject, float minimumRange, float maximumRange)
        {
            if (subject > maximumRange) return maximumRange;
            if (subject < minimumRange) return minimumRange;

            return subject;
        }
    }
}
