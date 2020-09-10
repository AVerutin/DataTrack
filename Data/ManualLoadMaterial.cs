using System;
using System.ComponentModel.DataAnnotations;

namespace DataTrack.Data
{
    public class ManualLoadMaterial
    {
        public string BunkerId { get; set; }

        public ManualLoadMaterial()
        {
            BunkerId = "0";
        }
    }
}