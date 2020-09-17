using System;
using System.Threading.Tasks;
using DataTrack.Data;

namespace DataTrack
{
    public class KernelService
    {
        public Kernel DataKernel;
        // public event Func<string, Task> kernel;

        public KernelService()
        {
            if (DataKernel == null)
            {
                DataKernel = new Kernel();
            }
        }
        
        // Can be called from anywhere
        public async Task Update(string value)
        {
            // Notify me
        }
    }
}