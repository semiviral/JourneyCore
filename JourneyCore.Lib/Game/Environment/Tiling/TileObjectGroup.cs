namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public class TileObjectGroup
    {
        public string DrawOrder { get; set; }
        public string Name { get; set; }
        public TileObject[] Objects { get; set; }
        public bool Visible { get; set; }
        public string Type { get; set; }
        public float Opacity { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
}