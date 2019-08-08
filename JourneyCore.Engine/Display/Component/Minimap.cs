using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Minimap : IUiObject, IHoverable, IScrollable
    {
        public Minimap()
        {
            VArray = new VertexArray(PrimitiveType.Quads);
            MinimapObjects = new Dictionary<uint, DrawObject>();
        }

        public VertexArray VArray { get; }
        public Dictionary<uint, DrawObject> MinimapObjects { get; }

        public Vector2f Size { get; set; }

        public bool IsHovered { get; private set; }
        public event EventHandler<MouseMoveEventArgs> Entered;
        public event EventHandler<MouseMoveEventArgs> Exited;

        public void OnMouseMoved(object sender, MouseMoveEventArgs args)
        {
            throw new NotImplementedException();
        }

        public event EventHandler<MouseWheelScrollEventArgs> Scrolled;

        public void OnMouseScrolled(object sender, MouseWheelScrollEventArgs args)
        {
            throw new NotImplementedException();
        }

        Vector2u IUiObject.Size { get; set; }

        public Vector2f Position { get; set; }
        public Vector2f Origin { get; set; }
        public Margin Margins { get; set; }
        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUiObject> SubscribableObjects()
        {
            throw new NotImplementedException();
        }

        public void OnParentResized(object sender, SizeEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void AddMinimapEntity(DrawObject drawObj)
        {
            if (!drawObj.Batchable || !(drawObj.Object is RectangleShape))
            {
                return;
            }

            RectangleShape _castedShape = (RectangleShape) drawObj.Object;

            drawObj.RecalculateVertices += OnMinimapEntityVerticesUpdated;

            _castedShape.Origin = _castedShape.Size / 2f;

            uint _startIndex = VArray.VertexCount;
            VArray.Resize(_startIndex + 4);

            if (VArray.VertexCount < _startIndex)
            {
                throw new IndexOutOfRangeException($"Index `{_startIndex}` out of range of VArray.");
            }

            drawObj.StartIndex = drawObj.StartIndex == 0 ? _startIndex : drawObj.StartIndex;
            MinimapObjects.Add(_startIndex, drawObj);

            CalculateVerticesAtIndex(_startIndex);
        }

        public void CalculateVerticesAtIndex(uint startIndex)
        {
            if (!MinimapObjects.Keys.Contains(startIndex))
            {
                return;
            }

            Vertex[] _vertices = MinimapObjects[startIndex].GetVertices(default).ToArray();

            for (uint _i = 0; _i < _vertices.Length; _i++)
            {
                VArray[startIndex + _i] = _vertices[_i];
            }
        }

        public void OnMinimapEntityVerticesUpdated(object sender, uint startIndex)
        {
            CalculateVerticesAtIndex(startIndex);
        }
    }
}