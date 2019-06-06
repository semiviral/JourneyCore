using System;
using JourneyCore.Lib.Game.Object;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Graphics.Drawing
{
    public class DrawObject : Transformable, Drawable, IAnchorable
    {
        public uint StartIndex { get; set; }
        public bool Batchable { get; set; }

        public Type ObjectType { get; }
        public object Object { get; }
        public Func<Vertex[]> GetVertices { get; }

        public Drawable Drawable => (Drawable)Object;
        public Transformable Transformable => (Transformable)Object;

        public DrawObject(uint startIndex, Type entityObjType, object entityObj, Func<Vertex[]> getVertices = null) :
            this(entityObjType, entityObj, getVertices)
        {
            StartIndex = startIndex;
        }

        public DrawObject(Type objectType, object obj, Func<Vertex[]> getVertices = null)
        {
            ObjectTypeChecks(objectType, obj);

            ObjectType = objectType;
            Object = obj;

            if (getVertices == null)
            {
                Batchable = false;
            }
            else
            {
                Batchable = true;

                GetVertices = getVertices;
            }
        }

        public new Vector2f Position
        {
            get => Transformable.Position;
            set
            {
                Transformable.Position = value;
                OnPositionChanged(Transformable.Position);
            }
        }

        public new float Rotation
        {
            get => Transformable.Rotation;
            set
            {
                Transformable.Rotation = value;
                OnRotationChanged(Transformable.Rotation);
            }
        }

        private static void ObjectTypeChecks(Type entityObjType, object entityObj)
        {
            if (!(entityObj is Drawable))
            {
                throw new InvalidCastException(
                    "Provided object is not safely castable to required type (Drawable).");
            }

            // test if type is assignable to object
            Convert.ChangeType(entityObj, entityObjType);
        }

        #region EVENTS

        public event EventHandler<uint> RecalculateVertices;
        public event EventHandler<Vector2f> PositionChanged;
        public event EventHandler<float> RotationChanged;

        private void OnRecalculateVertices()
        {
            RecalculateVertices?.Invoke(this, StartIndex);
        }

        private void OnPositionChanged(Vector2f newPosition)
        {
            PositionChanged?.Invoke(this, newPosition);
            OnRecalculateVertices();
        }

        private void OnRotationChanged(float newRotation)
        {
            RotationChanged?.Invoke(this, newRotation);
            OnRecalculateVertices();
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            Drawable.Draw(target, states);
        }

        #endregion
    }
}