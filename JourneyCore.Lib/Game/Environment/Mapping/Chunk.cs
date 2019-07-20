using JourneyCore.Lib.Game.Environment.Tiling;

namespace JourneyCore.Lib.Game.Environment.Mapping
{
    public class Chunk
    {
        public Chunk(int sizeX, int sizeY, int left, int top, int layer)
        {
            Left = left;
            Top = top;
            Layer = layer;

            // intiailise internal chunk array
            InternalChunk = new TilePrimitive[sizeX][];

            for (int y = 0; y < InternalChunk.Length; y++) InternalChunk[y] = new TilePrimitive[sizeY];
        }

        public TilePrimitive[][] InternalChunk { get; set; }
        public int Length => InternalChunk.Length;
        public int Left { get; set; }
        public int Top { get; set; }
        public int Layer { get; set; }

        public TilePrimitive[] this[int indexX]
        {
            get => InternalChunk[indexX];
            set => InternalChunk[indexX] = value;
        }
    }
}