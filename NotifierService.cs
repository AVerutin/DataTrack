using System;
using System.Threading.Tasks;

namespace DataTrack
{
    public class NotifierService
    {
        // Can be called from anywhere
        public async Task Update(ushort key, double value)
        {
            if (Notify != null)
            {
                await Notify.Invoke(key, value);
            }
        }

        public event Func<ushort, double, Task> Notify;    }
}
