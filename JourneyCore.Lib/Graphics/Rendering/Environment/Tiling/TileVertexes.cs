using SFML.Graphics;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class TileVertexes
    {
        public int DrawingLayer { get; }
        public uint VArrayIndex { get; }
        public Vertex[] Vertexes { get; set; }

        public TileVertexes(int drawingLayer, uint vArrayIndex, Vertex v0, Vertex v1, Vertex v2, Vertex v3)
        {
            DrawingLayer = drawingLayer;
            VArrayIndex = vArrayIndex;
            Vertexes = new Vertex[4];
            Vertexes[0] = v0;
            Vertexes[1] = v1;
            Vertexes[2] = v2;
            Vertexes[3] = v3;
        }
    }
}
