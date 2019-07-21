using System;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.System.Static;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawObject : Transformable, Drawable, IAnchorable
    {
        public DrawObject(uint startIndex, object entityObj, Func<Vertex[]> getVertices = null) :
            this(entityObj, getVertices)
        {
            StartIndex = startIndex;
        }

        public DrawObject(object obj, Func<Vertex[]> getVertices = null)
        {
            ObjectTypeChecks(obj);
            AssignSetOpacityMethod(obj);

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

        public uint StartIndex { get; set; }
        public bool Batchable { get; set; }

        public object Object { get; }
        public Func<Vertex[]> GetVertices { get; }

        public Action<sbyte> ModifyOpacity { get; set; }

        public Drawable Drawable => (Drawable) Object;
        public Transformable Transformable => (Transformable) Object;

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


        private void AssignSetOpacityMethod(object obj)
        {
            if (obj is VertexArray vArrayObj)
            {
                ModifyOpacity = arg1 => { vArrayObj.ModifyOpacity(arg1, 10); };
            }
            else if (obj is Shape shapeObj)
            {
                ModifyOpacity = arg1 =>
                {
                    Color modifiedColor = shapeObj.FillColor;

                    if ((modifiedColor.A + arg1) < 10)
                    {
                        modifiedColor.A = 10;
                    }
                    else if ((modifiedColor.A + arg1) > 255)
                    {
                        modifiedColor.A = 255;
                    }
                    else
                    {
                        modifiedColor.A = (byte) (modifiedColor.A + arg1);
                    }

                    shapeObj.FillColor = modifiedColor;
                };
            }
            else if (obj is Sprite spriteObj)
            {
                ModifyOpacity = arg1 =>
                {
                    Color modifiedColor = spriteObj.Color;

                    if ((modifiedColor.A + arg1) < 10)
                    {
                        modifiedColor.A = 10;
                    }
                    else if ((modifiedColor.A + arg1) > 255)
                    {
                        modifiedColor.A = 255;
                    }
                    else
                    {
                        modifiedColor.A = (byte) (modifiedColor.A + arg1);
                    }

                    spriteObj.Color = modifiedColor;
                };
            }
        }

        private static void ObjectTypeChecks(object obj)
        {
            if (!(obj is Drawable))
            {
                throw new InvalidCastException(
                    "Provided object is not safely castable to required type (Drawable).");
            }
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