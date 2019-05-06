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
