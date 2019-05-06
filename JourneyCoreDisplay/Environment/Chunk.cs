using JourneyCoreDisplay.Drawing;
using JourneyCoreDisplay.Sprites;
using SFML.Graphics;
using SFML.System;

namespace JourneyCoreDisplay.Environment
{
    public class Chunk
    {
        public int[][] ChunkData { get; set; }

        public Chunk() { }

        public Chunk(int[][] chunkData)
        {
            ChunkData = chunkData;
        }
    }
}
