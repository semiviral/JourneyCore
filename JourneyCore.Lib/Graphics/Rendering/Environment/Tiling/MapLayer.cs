using System.Xml.Serialization;

namespace JourneyCore.Lib.Graphics.Rendering.Environment.Tiling
{
    public class MapLayer
    {
        [XmlAttribute("id")]
        public short Id { get; set; }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("width")]
        public short Width { get; set; }

        [XmlAttribute("height")]
        public short Height { get; set; }

        [XmlElement("data")]
        public PrimitiveTile[] Data { get; set; }

        [XmlIgnore]
        public Chunk[][] Map { get; set; }

        public Chunk[][] CreateMap(short chunkSizeX, short chunkSizeY)
        {
            int widthInChunks = Width / chunkSizeX;
            int heightInChunks = Height / chunkSizeY;

            Map = new Chunk[widthInChunks][];

            for (int chunkY = 0; chunkY < heightInChunks; chunkY++)
            for (int chunkX = 0; chunkX < widthInChunks; chunkX++)
            {
                Map[chunkX] = new Chunk[heightInChunks];

                for (int y = 0; y < chunkSizeX; y++)
                for (int x = 0; x < chunkSizeY; x++)
                    Map[chunkX][chunkY] = new Chunk(chunkSizeX, chunkSizeY)
                    {
                        [x] = { [y] = Data[(chunkY + y) * heightInChunks + chunkX * widthInChunks + x] }
                    };
            }

            return Map;
        }
    }
}