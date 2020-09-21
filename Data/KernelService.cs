using System;
using System.Threading.Tasks;
using DataTrack.Data;

namespace DataTrack
{
    public class KernelService
    {
        public Kernel Data;
        // public event Func<string, Task> kernel;

        public KernelService()
        {
            if (Data == null)
            {
                Data = new Kernel();
            }
        }
        
        // Can be called from anywhere
        // public async Task Update(string value)
        // {
        //     // Notify me
        // }
    }
}