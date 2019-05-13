using SFML.System;

namespace JourneyCoreLib.System.Math
{
    public class MovementVector
    {
        public Vector2f Vector { get; }

        public MovementVector(Vector2f vector)
        {
            Vector = vector;
        }

        public MovementVector(float x, float y) : this(new Vector2f(x, y)) { }

        public static MovementVector operator +(MovementVector v1, MovementVector v2)
        {
            return new MovementVector((v1.Vector.X + v2.Vector.X) * 0.5f, (v1.Vector.Y + v2.Vector.Y) * 0.5f);
        }

        public static MovementVector operator -(MovementVector v1, MovementVector v2)
        {
            return new MovementVector(v1.Vector.X - ((v1.Vector.X + v2.Vector.X) * 0.5f), v1.Vector.Y - ((v1.Vector.Y + v2.Vector.Y) * 0.5f));
        }

        public static MovementVector operator +(MovementVector v1, Vector2f v2)
        {
            return new MovementVector((v1.Vector.X + v2.X) * 0.5f, (v1.Vector.Y + v2.Y) * 0.5f);
        }

        public static MovementVector operator -(MovementVector v1, Vector2f v2)
        {
            return new MovementVector(v1.Vector.X - ((v1.Vector.X + v2.X) * 0.5f), v2.Y - ((v1.Vector.Y + v2.Y) * 0.5f));
        }

        public static bool operator ==(MovementVector v1, Vector2f v2)
        {
            return v1.Vector == v2;
        }

        public static bool operator !=(MovementVector v1, Vector2f v2)
        {
            return v1.Vector != v2;
        }
    }
}
