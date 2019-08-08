namespace JourneyCore.Lib.Game.Environment.Tiling
{
    public struct TilePrimitive
    {
        public int Gid { get; set; }
        public int Rotation { get; set; }

        public TilePrimitive(int gid, int rotation)
        {
            Gid = gid;
            Rotation = rotation;
        }
    }
}