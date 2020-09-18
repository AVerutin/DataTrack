using System.Collections.Generic;

namespace DataTrack.Data
{
    public interface IIngot
    {
        public void AddParameter(int key, string value);
        public void AddParameter(int key, double value);

        public void SetDbId(long dbid);
    }
}