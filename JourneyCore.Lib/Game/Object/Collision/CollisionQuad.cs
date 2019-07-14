using System;
using JourneyCore.Lib.System.Math;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Game.Object.Collision
{
    public class CollisionQuad : ICollidable, IAnchorable
    {
        private Vector2f _Position;
        private Vector2f _Size;
        private float _Rotation;
        private Vector2f _Scale;

        public Vector2f Position
        {
            get => _Position;
            set
            {
                if (_Position == value)
                {
                    return;
                }

                _Position = value;
                UpdateShape();
            }
        }

        public Vector2f Size
        {
            get => _Size;
            set
            {
                if (_Size == value)
                {
                    return;
                }

                _Size = value;
                UpdateShape();
            }
        }

        public float Rotation
        {
            get => _Rotation;
            set
            {
                if (_Rotation == value)
                {
                    return;
                }

                _Rotation = value; 
                UpdateShape();
            }
        }

        public Vector2f Scale
        {
            get => _Scale;
            set
            {
                if (_Scale == value)
                {
                    return;
                }

                _Scale = value;
                _Position = new Vector2f(_Position.X * _Scale.X, _Position.Y * _Scale.Y);
                _Size = new Vector2f(_Size.X * _Scale.X, _Size.Y * _Scale.Y);

                UpdateShape();
            }
        }


        public Vector2f[] Vertices { get; private set; }
        public Vector2f CenterPoint { get; private set; }

        public bool Mobile { get; set; }

        public event EventHandler<Vector2f> Colliding; 

        public CollisionQuad()
        {
            Vertices = new Vector2f[4];

            _Position = _Size = CenterPoint = new Vector2f(0f, 0f);
            _Rotation = 0f;
        }

        public CollisionQuad(FloatRect square, float rotation)
        {
            Vertices = new Vector2f[4];

            _Position = new Vector2f(square.Left, square.Top);
            _Size = new Vector2f(square.Width, square.Height);
            _Rotation = rotation;
            CenterPoint = new Vector2f(0f, 0f);

            UpdateShape();
        }

        public CollisionQuad(CollisionQuad copy)
        {
            Vertices = new Vector2f[4];

            Position = copy.Position;
            Size = copy.Size;
            CenterPoint = copy.CenterPoint;
            CenterPoint = new Vector2f(0f, 0f);

            UpdateShape();
        }

        public void FlagCollision(object sender, Vector2f displacement)
        {
            Colliding?.Invoke(sender, displacement);
        }

        #region PRIVATE

        private void UpdateShape()
        {
            CenterPoint = Position + Size/ 2f;

            // top left point
            Vertices[0] = GraphMath.RotatePoint(Position, CenterPoint, Rotation);
            // top right point
            Vertices[1] = GraphMath.RotatePoint(new Vector2f(Position.X + Size.X, Position.Y), CenterPoint, Rotation);
            // bottom right point
            Vertices[2] = GraphMath.RotatePoint(new Vector2f(Position.X + Size.X, Position.Y + Size.Y), CenterPoint,
                Rotation);
            // bottom left point
            Vertices[3] = GraphMath.RotatePoint(new Vector2f(Position.X, Position.Y + Size.Y), CenterPoint, Rotation);
        }

        #endregion
    }
}