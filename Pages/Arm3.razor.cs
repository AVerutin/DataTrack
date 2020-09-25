using System.Collections.Generic;
using System.Threading.Tasks;
using DataTrack.Data;
using DataTrack.Shared;

namespace DataTrack.Pages
{
    public partial class Arm3
    {
        private (string value, Task t) lastNotification;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
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
