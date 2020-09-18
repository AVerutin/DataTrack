using System;
using System.Threading.Tasks;
using DataTrack.Data;

namespace DataTrack.Pages
{
    public partial class Arm2
    {
        private (string value, Task t) lastNotification;
        private string _material1;
        private string _material2;
        private string _material3;

        protected override void OnInitialized()
        {
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            var input1 = DataKernel.DataKernel.GetInputTanker(0);
            var input2 = DataKernel.DataKernel.GetInputTanker(1);
            int kernelMaterialIndex = DataKernel.DataKernel.GetCurrentMaterialIndex();
            _material3 = DataKernel.DataKernel.GetMaterial(kernelMaterialIndex).ToString();
            
            _material1 = input1.GetMaterialName();
            _material2 = input2.GetMaterialName();
        }
        
        private async Task OnNotify(string value)
        {
            await InvokeAsync(() =>
            {
                lastNotification.value = value;
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }
    }
}
