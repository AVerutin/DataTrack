namespace DataTrack.Data
{
    public class Chemical
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public string Sign { get; private set; }
        public double Volume { get; private set; }

        public Chemical()
        {
            Id = default;
            Name = default;
            Sign = default;
            Volume = default;
        }

        public Chemical(long id)
        {
            if (id > 0)
            {
                Id = id;
                Name = default;
                Sign = default;
                Volume = default;
            }
        }

        public Chemical(long id, string name, string sign, double volume)
        {
            Id = id;
            Name = name;
            Sign = sign;
            Volume = volume;
        }

        public override string ToString()
        {
            return $"{Name}[{Sign}, {Volume:F4}]";
        }
    }
}