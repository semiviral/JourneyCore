using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.Game.Object.Entity;
using JourneyCore.Lib.System.Math;
using JourneyCore.Lib.System.Static;
using SFML.Graphics;
using SFML.System;

namespace JourneyCore.Lib.Display.Drawing
{
    public class DrawView : IAnchorable
    {
        public const float DEFAULT_VIEW_ROTATION = 180f;
        private float _ZoomFactor = 1.0f;

        public DrawView(DrawViewLayer layer, View view, bool visible = false)
        {
            Layer = layer;
            View = view;
            Visible = visible;

            DrawQueue = new SortedList<int, List<DrawItem>>();
            DefaultSize = View.Size;
        }

        public DrawViewLayer Layer { get; }
        public View View { get; }
        public bool Visible { get; set; }

        private SortedList<int, List<DrawItem>> DrawQueue { get; }
        private Vector2f DefaultSize { get; }

        public float ZoomFactor
        {
            get => _ZoomFactor;
            set
            {
                _ZoomFactor = value.LimitToRange(1.0f, 10f);

                View.Size = DefaultSize * _ZoomFactor;
            }
        }

        public Vector2f Position
        {
            get => View.Center;
            set => View.Center = value;
        }

        public float Rotation
        {
            get => View.Rotation;
            set => View.Rotation = value + (DEFAULT_VIEW_ROTATION % 360);
        }

        public void ModifyOpacity(sbyte alphaModifier)
        {
            foreach ((int _layer, List<DrawItem> _drawItems) in DrawQueue)
            foreach (DrawItem _drawItem in _drawItems)
            {
                _drawItem.Subject.ModifyOpacity?.Invoke(alphaModifier);
            }
        }

        public void AddDrawItem(int layer, DrawItem drawItem)
        {
            if (!DrawQueue.Keys.Contains(layer))
            {
                DrawQueue.Add(layer, new List<DrawItem>());
            }

            DrawQueue[layer].Add(drawItem);
        }

        public void Draw(RenderWindow window, float frameTime)
        {
            DateTime _absoluteNow = DateTime.Now;

            if (DrawQueue.Count <= 0)
            {
                return;
            }

            foreach ((int _key, List<DrawItem> _drawItemsPrelim) in DrawQueue)
            {
                _drawItemsPrelim.RemoveAll(drawItem =>
                    (drawItem == null) ||
                    ((drawItem.MaxLifetime.Ticks != DateTime.MinValue.Ticks) &&
                     (drawItem.MaxLifetime.Ticks < _absoluteNow.Ticks)));

                foreach ((RenderStates _renderStates, List<DrawItem> _drawItems) in _drawItemsPrelim.GroupBy(item =>
                        item.SubjectRenderStates, item => item,
                    (states, items) => new KeyValuePair<RenderStates, List<DrawItem>>(states, items.ToList())))
                {
                    VertexArray _vArray = new VertexArray(PrimitiveType.Quads);

                    foreach (DrawItem _drawItem in _drawItems)
                    {
                        _drawItem.PreDraw?.Invoke(frameTime);

                        if (!_drawItem.Subject.Batchable)
                        {
                            window.Draw(_drawItem.Subject, _renderStates);

                            continue;
                        }

                        Vertex[] _vertices = _drawItem.Subject.GetVertices(_drawItem.Subject.Position).ToArray();

                        uint _startIndex = _vArray.VertexCount;
                        _vArray.Resize((uint)(_vArray.VertexCount + _vertices.Length));
                        
                        for (uint _i = 0; _i < _vertices.Length; _i++)
                        {
                            _vArray[_startIndex + _i] = _vertices[_i];
                        }
                    }

                    if (_vArray.VertexCount == 0)
                    {
                        continue;
                    }

                    window.Draw(_vArray, _renderStates);
                }
            }
        }

        #region EVENTS

        public void OnAnchorPositionChanged(object sender, EntityPositionChangedEventArgs args)
        {
            Position = args.NewPosition;
        }

        public void OnAnchorRotationChanged(object sender, float rotation)
        {
            Rotation = rotation;
        }

        #endregion
    }
}