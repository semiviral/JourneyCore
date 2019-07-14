using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Display.Drawing;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace JourneyCore.Lib.Display.Component
{
    public class Minimap : IUIObject, IHoverable, IScrollable
    {
        private Vector2u _Size;
        public VertexArray VArray { get; }
        public Dictionary<uint, DrawObject> MinimapObjects { get; }

        public Minimap()
        {
            VArray = new VertexArray(PrimitiveType.Quads);
            MinimapObjects = new Dictionary<uint, DrawObject>();
        }

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

            RectangleShape castedShape = (RectangleShape)drawObj.Object;

            drawObj.RecalculateVertices += OnMinimapEntityVerticesUpdated;

            castedShape.Origin = castedShape.Size / 2f;

            uint startIndex = VArray.VertexCount;
            VArray.Resize(startIndex + 4);

            if (VArray.VertexCount < startIndex)
            {
                throw new IndexOutOfRangeException($"Index `{startIndex}` out of range of VArray.");
            }

            drawObj.StartIndex = drawObj.StartIndex == 0 ? startIndex : drawObj.StartIndex;
            MinimapObjects.Add(startIndex, drawObj);

            CalculateVerticesAtIndex(startIndex);
        }

        public void CalculateVerticesAtIndex(uint startIndex)
        {
            if (!MinimapObjects.Keys.Contains(startIndex))
            {
                return;
            }

            Vertex[] vertices = MinimapObjects[startIndex].GetVertices();

            VArray[startIndex + 0] = vertices[0];
            VArray[startIndex + 1] = vertices[1];
            VArray[startIndex + 2] = vertices[2];
            VArray[startIndex + 3] = vertices[3];
        }

        public void OnMinimapEntityVerticesUpdated(object sender, uint startIndex)
        {
            CalculateVerticesAtIndex(startIndex);
        }

        public Vector2f Size { get; set; }

        Vector2u IUIObject.Size
        {
            get => _Size;
            set => _Size = value;
        }

        public Vector2f Position { get; set; }
        public Vector2f Origin { get; set; }
        public event EventHandler<SizeEventArgs> Resized;

        public IEnumerable<IUIObject> SubscribableObjects()
        {
            throw new NotImplementedException();
        }
    }
}