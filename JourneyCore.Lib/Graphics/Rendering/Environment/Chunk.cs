using JourneyCore.Lib.Graphics.Rendering.Environment.Tiling;

namespace JourneyCore.Lib.Graphics.Rendering.Environment
{
    public class Chunk
    {
        private PrimitiveTile[][] InternalChunk { get; }
        public int Length => InternalChunk.Length;

        public Chunk(short sizeX, short sizeY)
        {
            // intiailise internal chunk array
            InternalChunk = new PrimitiveTile[sizeX][];

            for (int y = 0; y < InternalChunk.Length; y++)
            {
                InternalChunk[y] = new PrimitiveTile[sizeY];
            }
        }

        public PrimitiveTile[] this[int indexX]
        {
            get => InternalChunk[indexX];
            set => InternalChunk[indexX] = value;
        }
    }
}
