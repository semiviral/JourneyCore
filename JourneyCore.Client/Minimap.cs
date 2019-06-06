using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCore.Lib.Game.Object;
using JourneyCore.Lib.Graphics.Drawing;
using SFML.Graphics;

namespace JourneyCore.Client
{
    public class Minimap
    {
        public VertexArray VArray { get; }
        public Dictionary<uint, DrawObject> MinimapObjects { get; }

        public Minimap()
        {
            VArray = new VertexArray(PrimitiveType.Quads);
            MinimapObjects = new Dictionary<uint, DrawObject>();
        }

        public void AddMinimapEntity(DrawObject drawObj)
        {
            if (!drawObj.Batchable || drawObj.ObjectType != typeof(RectangleShape))
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

            CalculateVerticesByIndex(startIndex);
        }

        public void CalculateVerticesByIndex(uint startIndex)
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
            CalculateVerticesByIndex(startIndex);
        }
    }
}