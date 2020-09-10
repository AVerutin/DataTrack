using System;
using System.Threading.Tasks;

namespace DataTrack
{
    public class NotifierService
    {
        // Can be called from anywhere
        public async Task Update(string value)
        {
            if (Notify != null)
            {
                await Notify.Invoke(value);
            }
        }

        public event Func<string, Task> Notify;
    }
}
