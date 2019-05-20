namespace JourneyCore.Lib.Graphics.Rendering.Environment.Chunking
{
    public struct Chunk
    {
        public short[][] ChunkData { get; set; }

        public Chunk(short[][] chunkData)
        {
            ChunkData = chunkData;
        }
    }
}