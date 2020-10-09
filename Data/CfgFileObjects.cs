using System.Collections.Generic;

namespace DataTrack.Data
{
    public class CfgFileObjects
    {
        public int Uid { get; set; }
        public string Name { get; set; }
        public CfgFileObjectTypes Type { get; set; }
        public Dictionary<string,string> Parameters { get; set; }

        public CfgFileObjects()
        {
            Uid = -1;
            Name = "";
            Parameters = new Dictionary<string, string>();
            Type = CfgFileObjectTypes.Default;
        }

        public override string ToString()
        {
            return Name;
        }

    }
}