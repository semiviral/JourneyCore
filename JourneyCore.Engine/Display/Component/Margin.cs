namespace JourneyCore.Lib.Display.Component
{
    public class Margin
    {
        public Margin()
        {
            Top = Left = Bottom = Right = 0;
        }

        public Margin(uint xMargin, uint yMargin)
        {
            Top = Bottom = xMargin;
            Left = Right = yMargin;
        }

        public uint Top { get; set; }
        public uint Left { get; set; }
        public uint Bottom { get; set; }
        public uint Right { get; set; }
    }
}