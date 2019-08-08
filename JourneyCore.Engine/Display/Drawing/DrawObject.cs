using System;
using System.Collections.Generic;
using System.Numerics;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Static;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawObject : Transformable, Drawable, IAnchorable
    {
        public DrawObject(uint startIndex, object entityObj, Func<Vector2f, IEnumerable<Vertex>> getVertices = null) :
            this(entityObj, getVertices)
        {
            StartIndex = startIndex;
        }

        public DrawObject(object obj, Func<Vector2f, IEnumerable<Vertex>> getVertices = null)
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
        public Func<Vector2f, IEnumerable<Vertex>> GetVertices { get; }

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
        
        public new Vector2f Origin
        {
            get => Transformable.Origin;
            set => Transformable.Origin = value;
        }

        public new Vector2f Scale
        {
            get => Transformable.Scale;
            set => Transformable.Scale = value;
        }


        private void AssignSetOpacityMethod(object obj)
        {
            switch (obj)
            {
                case VertexArray _vArrayObj:
                    ModifyOpacity = arg1 => { _vArrayObj.ModifyOpacity(arg1, 10); };
                    break;
                case Shape _shapeObj:
                    ModifyOpacity = arg1 =>
                    {
                        Color _modifiedColor = _shapeObj.FillColor;

                        if ((_modifiedColor.A + arg1) < 10)
                        {
                            _modifiedColor.A = 10;
                        }
                        else if ((_modifiedColor.A + arg1) > 255)
                        {
                            _modifiedColor.A = 255;
                        }
                        else
                        {
                            _modifiedColor.A = (byte) (_modifiedColor.A + arg1);
                        }

                        _shapeObj.FillColor = _modifiedColor;
                    };
                    break;
                case Sprite _spriteObj:
                    ModifyOpacity = arg1 =>
                    {
                        Color _modifiedColor = _spriteObj.Color;

                        if ((_modifiedColor.A + arg1) < 10)
                        {
                            _modifiedColor.A = 10;
                        }
                        else if ((_modifiedColor.A + arg1) > 255)
                        {
                            _modifiedColor.A = 255;
                        }
                        else
                        {
                            _modifiedColor.A = (byte) (_modifiedColor.A + arg1);
                        }

                        _spriteObj.Color = _modifiedColor;
                    };
                    break;
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

        public void OnAnchorPositionChanged(object sender, EntityPositionChangedEventArgs args)
        {
            Position = args.NewPosition;
        }

        public void OnAnchorRotationChanged(object sender, float rotation)
        {
            Rotation = rotation;
        }

        #endregion
        
        public void Draw(RenderTarget target, RenderStates states)
        {
            Drawable.Draw(target, states);
        }
    }
}