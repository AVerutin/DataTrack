using System;
using System.ComponentModel.DataAnnotations;

namespace DataTrack.Data
{
    public class ManualWork
    {
        public string MaterialID { get; set; }
        public string BunkerID { get; set; }

        public ManualWork()
        {
            MaterialID = "0";
            BunkerID = "0";
        }
    }
}