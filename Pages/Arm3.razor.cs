using System.Threading.Tasks;

namespace DataTrack.Pages
{
    public partial class Arm3
    {
        private (ushort key, double value) lastNotification;

        protected override void OnInitialized()
        {
            Notifier.Notify += OnNotify;
            
            // Точка входа на страницу
            
        }
        
        private async Task OnNotify(ushort key, double value)
        {
            await InvokeAsync(() =>
            {
                lastNotification = (key, value);
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            Notifier.Notify -= OnNotify;
        }

    }
}
