using System;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Collision
{
    public class CollisionQuad : ICollidable, IAnchorable
    {
        public CollisionQuad()
        {
            Vertices = new Vector2f[4];

            InnerRect = new RectangleShape
            {
                Position = Size = new Vector2f(0f, 0f),
                Rotation = 0f
            };
        }

        public CollisionQuad(FloatRect square, float rotation)
        {
            Vertices = new Vector2f[4];

            InnerRect = new RectangleShape
            {
                Size = new Vector2f(square.Width, square.Height),
                Position = new Vector2f(square.Left + (square.Width / 2f), square.Top + (square.Height / 2f)),
                Rotation = rotation
            };
            InnerRect.Origin = InnerRect.Size / 2f;

            UpdateShape();
        }

        public CollisionQuad(CollisionQuad copy)
        {
            Vertices = new Vector2f[4];

            InnerRect = new RectangleShape
            {
                Size = copy.Size,
                Position = copy.Position,
                Rotation = copy.Rotation
            };
            InnerRect.Origin = copy.Size / 2f;

            UpdateShape();
        }

        private RectangleShape InnerRect { get; }

        public Vector2f Size
        {
            get => InnerRect.Size;
            set
            {
                if (InnerRect.Size == value)
                {
                    return;
                }

                InnerRect.Size = value;

                UpdateShape();
            }
        }

        public Vector2f Scale
        {
            get => InnerRect.Scale;
            set
            {
                if (InnerRect.Scale == value)
                {
                    return;
                }

                InnerRect.Scale = value;

                UpdateShape();
            }
        }


        public Vector2f[] Vertices { get; }

        public bool Mobile { get; set; }

        public float Rotation
        {
            get => InnerRect.Rotation;
            set
            {
                if (InnerRect.Rotation == value)
                {
                    return;
                }

                InnerRect.Rotation = value;

                UpdateShape();
            }
        }

        public Vector2f Position
        {
            get => InnerRect.Position;
            set
            {
                if (InnerRect.Position == value)
                {
                    return;
                }

                InnerRect.Position = value;

                UpdateShape();
            }
        }

        public Vector2f Origin
        {
            get
                => InnerRect.Origin;

            private set
            {
                if (InnerRect.Origin == value)
                {
                    return;
                }

                InnerRect.Origin = value;

                UpdateShape();
            }
        }

        public event EventHandler<Vector2f> Colliding;

        private void UpdateShape()
        {
            Origin = Size / 2f;


            // top left point
            Vertices[0] = GraphMath.RotatePoint(Position, Origin, Rotation);
            // top right point
            Vertices[1] = GraphMath.RotatePoint(new Vector2f(Position.X + Size.X, Position.Y), Origin, Rotation);
            // bottom right point
            Vertices[2] = GraphMath.RotatePoint(new Vector2f(Position.X + Size.X, Position.Y + Size.Y), Origin,
                Rotation);
            // bottom left point
            Vertices[3] = GraphMath.RotatePoint(new Vector2f(Position.X, Position.Y + Size.Y), Origin, Rotation);
        }

        public FloatRect GetGlobalBounds()
        {
            return InnerRect.GetGlobalBounds();
        }

        public FloatRect GetLocalBounds()
        {
            return InnerRect.GetLocalBounds();
        }

        public bool Intserects(CollisionQuad testQuad)
        {
            return GetGlobalBounds().Intersects(testQuad.GetGlobalBounds());
        }

        public bool Intersects(CollisionQuad quad, out FloatRect overlap)
        {
            return GetGlobalBounds().Intersects(quad.GetGlobalBounds(), out overlap);
        }

        public void FlagCollision(object sender, Vector2f displacement)
        {
            Colliding?.Invoke(sender, displacement);
        }
    }
}