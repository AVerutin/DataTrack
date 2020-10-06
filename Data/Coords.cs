namespace DataTrack.Data
{
    public class Coords
    {
        public double PosX { get; set; }
        public double PosY { get; set; }

        public Coords()
        {
            PosX = default;
            PosY = default;
        }

        public Coords(double posX, double posY)
        {
            PosX = posX;
            PosY = posY;
        }
    }
}