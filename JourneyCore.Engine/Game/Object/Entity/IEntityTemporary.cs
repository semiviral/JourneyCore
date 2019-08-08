using System;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public interface IEntityTemporary
    {
        long Lifetime { get; }
        DateTime MaximumLifetime { get; }

        DateTime TriggerAlive();
    }
}