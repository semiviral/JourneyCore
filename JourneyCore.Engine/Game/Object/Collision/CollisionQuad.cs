using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Collision
{
    public class CollisionQuad : RectangleShape, ICollidable, IAnchorable
    {
        public CollisionQuad()
        {
        }

        public CollisionQuad(RectangleShape copy) : base(copy)
        {
        }

        public CollisionQuad(FloatRect square, float rotation) : base(new Vector2f(square.Width, square.Height))
        {
            Position = new Vector2f(square.Left, square.Top);
            Rotation = rotation;
        }

        public bool Mobile { get; set; }

        #region EVENTS

        public event EventHandler<Vector2f> Colliding;

        public void OnAnchorPositionChanged(object sender, EntityPositionChangedEventArgs args)
        {
            Position = args.NewPosition - Origin.MultiplyBy(Scale);
        }

        public void OnAnchorRotationChanged(object sender, float rotation)
        {
            Rotation = rotation;
        }

        #endregion

        public IEnumerable<Vector2f> GetAllPoints()
        {
            for (uint _i = 0; _i < GetPointCount(); _i++)
            {
                yield return GraphMath.RotatePoint(GetPoint(_i), Origin, Rotation);
            }
        }

        public IEnumerable<Vector2f> GetAllPointsScaled()
        {
            return GetAllPoints().Select(point => point.MultiplyBy(Scale));
        }
    }
}