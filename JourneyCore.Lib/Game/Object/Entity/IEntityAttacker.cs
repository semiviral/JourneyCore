using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;

namespace JourneyCore.Lib.Game.Object.Entity
{
    public interface IEntityAttacker
    {
        /// <summary>
        ///     Value which is added to AttackCooldown when entity attacks in milliseconds
        /// </summary>
        int AttackCooldownValue { get; }

        bool CanAttack { get; }
        RenderStates ProjectileRenderStates { get; }

        DrawItem FireProjectile(double centerRelativeMouseX, double centerRelativeMouseY, int tileWidth);
    }
}