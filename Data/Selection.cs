namespace DataTrack.Data
{
    public class Selection
    {
        public bool Activate { get; set; }
        public string Selected { get; set; }
        public string Deselected { get; set; }

        public Selection()
        {
            Activate = false;
            Selected = "";
            Deselected = "";
        }
    }
}