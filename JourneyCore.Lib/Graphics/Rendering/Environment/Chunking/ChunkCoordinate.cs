namespace JourneyCore.Lib.Graphics.Rendering.Environment.Chunking
{
    public class ChunkCoordinate
    {
        public ChunkCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }
}